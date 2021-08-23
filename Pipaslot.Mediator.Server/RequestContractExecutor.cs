using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Contracts;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Server side executed receiving CommandQueryContract object through network connection
    /// </summary>
    public class RequestContractExecutor
    {
        private static JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };
        private readonly IMediator _mediator;
        private readonly string _version;

        public RequestContractExecutor(IMediator mediator, string version = "")
        {
            _mediator = mediator;
            _version = version;
        }

        public async Task<string> ExecuteQuery(MediatorRequestSerializable request, CancellationToken cancellationToken)
        {
            var queryType = Type.GetType(request.ObjectName);
            if (queryType == null)
            {
                throw new Exception($"Can not recognize type {request.ObjectName}");
            }
            var query = JsonSerializer.Deserialize(request.Json, queryType);
            if (query == null)
            {
                throw new Exception($"Can not deserialize contract as type {request.ObjectName}");
            }
            if (query is IMediatorActionProvidingData req)
            {
                return await ExecuteRequest(req, cancellationToken).ConfigureAwait(false);
            }
            return await ExecuteMessage((IMediatorAction)query, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> ExecuteMessage(IMediatorAction message, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Dispatch(message, cancellationToken);
                return SerializeObject(result);
            }
            catch (Exception e)
            {
                // This should never happen because mediator handles errors internally. But need to prevent errors if somebody will override mediator behavior
                return SerializeError(e.Message);
            }
        }

        private async Task<string> ExecuteRequest(object query, CancellationToken cancellationToken)
        {
            var queryInterfaceType = typeof(IMediatorAction<>);
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
                // This should never happen because mediator handles errors internally. But need to prevent errors if somebody will override mediator behavior
                return SerializeError(e.Message);
            }
        }

        private string SerializeObject(object? result)
        {
            if (result is MediatorResponse mediatorResponse)
            {
                return SerializeResponse(mediatorResponse);
            }
            return SerializeError($"Unexpected result type from mediator pipeline. Was expected {typeof(MediatorResponse)} but {result?.GetType()} was returned instead.");
        }

        private string SerializeError(string errorMessage)
        {
            return SerializeResponse(new MediatorResponse(errorMessage));
        }

        private string SerializeResponse(MediatorResponse response)
        {
            if (_version == MediatorRequestSerializable.VersionHeaderValueV2)
            {
                var obj = new MediatorResponseSerializableV2
                {
                    ErrorMessages = response.ErrorMessages.ToArray(),
                    Results = response.Results
                    .Select(r => SerializerResult(r))
                    .ToArray(),
                    Success = response.Success
                };
                return JsonSerializer.Serialize(obj, _serializationOptions);
            }
            else
            {
                var obj = new MediatorResponseSerializable
                {
                    ErrorMessages = response.ErrorMessages.ToArray(),
                    Results = response.Results.ToArray(),
                    Success = response.Success
                };
                return JsonSerializer.Serialize(obj, _serializationOptions);
            }
        }

        private MediatorResponseSerializableV2.SerializedResult SerializerResult(object request)
        {
            return new MediatorResponseSerializableV2.SerializedResult
            {
                Json = JsonSerializer.Serialize(request, _serializationOptions),
                ObjectName = request.GetType().AssemblyQualifiedName
            };
        }

    }
}
