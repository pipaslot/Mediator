using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http
{
    public class MediatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServerMediatorOptions _option;
        private readonly IContractSerializer _serializer;

        public MediatorMiddleware(RequestDelegate next, ServerMediatorOptions option, IContractSerializer serializer)
        {
            _next = next;
            _option = option;
            _serializer = serializer;
        }

        public async Task Invoke(HttpContext context)
        {
            var method = context.Request.Method.ToUpper();
            var isPost = method == "POST";
            var isGet = method == "GET";
            if (context.Request.Path == _option.Endpoint && (isPost || isGet))
            {
                var mediatorResponse = await SafeExecute(context, isPost).ConfigureAwait(false);
                var serializedResponse = _serializer.SerializeResponse(mediatorResponse);
                // Change status code only if has default value (200: OK)
                if (context.Response.StatusCode == (int)HttpStatusCode.OK && mediatorResponse.Failure)
                {
                    context.Response.StatusCode = _option.ErrorHttpStatusCode;
                }
                if (!context.Response.HasStarted)
                {
                    context.Response.ContentType = "application/json; charset=utf-8";
                    await context.Response.WriteAsync(serializedResponse).ConfigureAwait(false);
                }
            }
            else
            {
                await _next(context).ConfigureAwait(false);
            }
        }

        private async Task<IMediatorResponse> SafeExecute(HttpContext context, bool isPost)
        {
            try
            {
                var body = isPost
                    ? await GetBody(context).ConfigureAwait(false)
                    : context.Request.Query.TryGetValue(MediatorConstants.ActionQueryParamName, out var actionQuery)
                        ? actionQuery.ToString()
                        : "";
                var mediator = CreateMediator(context);
                var action = _serializer.DeserializeRequest(body);
                var mediatorResponse = action is IMediatorActionProvidingData req
                    ? await ExecuteRequest(mediator, req, context.RequestAborted).ConfigureAwait(false)
                    : await ExecuteMessage(mediator, action, context.RequestAborted).ConfigureAwait(false);
                return mediatorResponse;
            }
            catch (Exception ex)
            {
                return new MediatorResponse(ex.Message);
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
            var body = await reader.ReadToEndAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(body))
            {
                throw MediatorHttpException.CreateForInvalidRequest(body);
            }
            return body;
        }

        private async Task<IMediatorResponse> ExecuteMessage(IMediator mediator, IMediatorAction message, CancellationToken cancellationToken)
        {
            try
            {
                return await mediator.Dispatch(message, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                // This should never happen because mediator handles errors internally. But need to prevent errors if somebody will override mediator behavior
                return new MediatorResponse(e.Message, message);
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
