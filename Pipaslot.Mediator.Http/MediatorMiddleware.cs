using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Threading;
using Pipaslot.Mediator.Http.Contracts;
using Pipaslot.Mediator.Http.Options;

namespace Pipaslot.Mediator.Http
{
    public class MediatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServerMediatorOptions _option;
        private readonly IContractSerializer _serializer;
        private readonly PipelineConfigurator _configurator;

        public MediatorMiddleware(RequestDelegate next, ServerMediatorOptions option, IContractSerializer serializer, PipelineConfigurator configurator)
        {
            _next = next;
            _option = option;
            _serializer = serializer;
            _configurator = configurator;
        }

        public async Task Invoke(HttpContext context)
        {
            var method = context.Request.Method.ToUpper();
            if (context.Request.Path == _option.Endpoint && method == "POST")
            {
                var mediator = CreateMediator(context);
                var body = await GetBody(context);

                var contract = _serializer.DeserializeRequest(body);
                var action = Validate(contract);

                var mediatorResponse = action is IMediatorActionProvidingData req
                ? await ExecuteRequest(mediator, req, context.RequestAborted)
                : await ExecuteMessage(mediator, (IMediatorAction)action, context.RequestAborted);

                var serializedResponse = _serializer.SerializeResponse(mediatorResponse);

                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(serializedResponse);
            }
            else
            {
                await _next(context);
            }
        }

        private IMediator CreateMediator(HttpContext context)
        {
            if (context.RequestServices.GetService(typeof(IMediator)) is not IMediator resolver)
            {
                throw MediatorHttpException.CreateForUnregisteredService(typeof(IMediator));
            }
            return resolver;
        }

        private async Task<string> GetBody(HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                throw MediatorHttpException.CreateForEmptyBody();
            }
            return body;
        }

        internal object Validate(MediatorRequestDeserialized request)
        {
            if (request.ActionType == null)
            {
                throw MediatorHttpException.CreateForUnrecognizedType(request.ObjectName);
            }
            var queryType = request.ActionType;
            if (request.Content == null)
            {
                throw MediatorHttpException.CreateForUnparsedContract();
            }
            var query = request.Content;
            if (!_configurator.ActionMarkerAssemblies.Contains(queryType.Assembly))
            {
                throw MediatorHttpException.CreateForUnregisteredType(queryType);
            }
            if (!typeof(IMediatorAction).IsAssignableFrom(queryType))
            {
                throw MediatorHttpException.CreateForNonContractType(queryType);
            }
            if (query == null)
            {
                throw new MediatorHttpException($"Can not deserialize contract as type {request.ObjectName}");
            }
            return request.Content;
        }

        private async Task<IMediatorResponse> ExecuteMessage(IMediator mediator, IMediatorAction message, CancellationToken cancellationToken)
        {
            try
            {
                return await mediator.Dispatch(message, cancellationToken);
            }
            catch (Exception e)
            {
                // This should never happen because mediator handles errors internally. But need to prevent errors if somebody will override mediator behavior
                return new MediatorResponse(e.Message);
            }
        }

        private async Task<IMediatorResponse> ExecuteRequest(IMediator mediator, object query, CancellationToken cancellationToken)
        {
            var resultType = RequestGenericHelpers.GetRequestResultType(query.GetType());
            if (resultType == null)
            {
                throw new MediatorHttpException($"Object {query.GetType()} is not assignable to type {typeof(IMediatorAction<>)}");
            }
            var method = mediator.GetType()
                    .GetMethod(nameof(IMediator.Execute))!
                .MakeGenericMethod(resultType);
            try
            {
                var task = (Task)method.Invoke(mediator, new[] { query, cancellationToken })!;
                await task.ConfigureAwait(false);

                var resultProperty = task.GetType().GetProperty("Result");
                var result = resultProperty?.GetValue(task);
                if (result is MediatorResponse mediatorResponse)
                {
                    return mediatorResponse;
                }
                return new MediatorResponse($"Unexpected result type from mediator pipeline. Was expected {typeof(MediatorResponse)} but {result?.GetType()} was returned instead.");
            }
            catch (Exception e)
            {
                // This should never happen because mediator handles errors internally. But need to prevent errors if somebody will override mediator behavior
                return new MediatorResponse(e.Message);
            }
        }
    }
}
