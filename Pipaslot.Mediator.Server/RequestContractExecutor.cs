using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Server side executed receiving CommandQueryContract object through network connection
    /// </summary>
    public class RequestContractExecutor
    {
        private readonly IMediator _mediator;

        public RequestContractExecutor(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<string> ExecuteQuery(MediatorRequestSerializable request, CancellationToken cancellationToken)
        {
            var queryType = Type.GetType(request.ObjectName);
            if (queryType == null)
            {
                throw new Exception($"Can not recognize type {request.ObjectName}");
            }
            var query = JsonSerializer.Deserialize(request.Json, queryType, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (query == null)
            {
                throw new Exception($"Can not deserialize contract as type {request.ObjectName}");
            }
            if (query is IMessage message)
            {
                return await ExecuteMessage(message, cancellationToken).ConfigureAwait(false);
            }
            return await ExecuteRequest(query, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> ExecuteMessage(IMessage message, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Dispatch(message, cancellationToken);
                return SerializeObject(result);
            }
            catch (Exception e)
            {
                return SerializeError(e.Message);
            }
        }

        private async Task<string> ExecuteRequest(object query, CancellationToken cancellationToken)
        {
            var queryInterfaceType = typeof(IRequest<>);
            var resultType = query.GetType()
                .GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == queryInterfaceType)
                ?.GetGenericArguments()
                .FirstOrDefault();
            if (resultType == null)
            {
                throw new Exception($"Object {query.GetType()} is not assignable to type {queryInterfaceType }");
            }

            var method = _mediator.GetType()
                    .GetMethod(nameof(IMediator.Execute))!
                .MakeGenericMethod(resultType);
            try
            {
                var task = (Task)method.Invoke(_mediator, new[] { query, cancellationToken })!;
                await task.ConfigureAwait(false);

                var resultProperty = task.GetType().GetProperty("Result");
                var result = resultProperty?.GetValue(task);
                return SerializeObject(result);
            }
            catch (Exception e)
            {
                return SerializeError(e.Message);
            }
        }
        private string SerializeObject(object? result)
        {
            if (result is MediatorResponse mediatorResponse)
            {
                return Serialize(mediatorResponse);
            }
            return SerializeError($"Unexpected result type from mediator pipeline. Was expected {typeof(MediatorResponse)} but {result?.GetType()} was returned instead.");
        }
        private string SerializeError(string errorMessage)
        {
            return Serialize(new MediatorResponse(errorMessage));
        }
        private string Serialize(MediatorResponse response)
        {
            var obj = new MediatorResponseSerializable
            {
                ErrorMessages = response.ErrorMessages.ToArray(),
                Results = response.Results.ToArray(),
                Success = response.Success
            };
            return JsonSerializer.Serialize(obj);
        }
    }
}
