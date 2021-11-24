using System;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Serialization;
using Pipaslot.Mediator.Server;
//TODO move into Server namespace
namespace Pipaslot.Mediator
{
    /// <summary>
    /// Server side executed receiving CommandQueryContract object through network connection
    /// </summary>
    /// TODO: make internal in next major version
    public class RequestContractExecutor
    {
        private readonly IMediator _mediator;
        private readonly PipelineConfigurator _configurator;
        private readonly IContractSerializer _serializer;

        public RequestContractExecutor(IMediator mediator, PipelineConfigurator configurator, string version = "")
        {
            _mediator = mediator;
            _configurator = configurator;
            _serializer = version == MediatorRequestSerializable.VersionHeaderValueV2
                ? new ContractSerializerV2()
                : new ContractSerializerV1();
        }

        public async Task<string> ExecuteQuery(MediatorRequestSerializable request, CancellationToken cancellationToken)
        {
            var queryType = Type.GetType(request.ObjectName);
            if (queryType == null)
            {
                throw MediatorServerException.CreateForUnrecognizedType(request.ObjectName);
            }
            if (!_configurator.ActionMarkerAssemblies.Contains(queryType.Assembly))
            {
                throw MediatorServerException.CreateForUnregisteredType(queryType);
            }
            if (!typeof(IMediatorAction).IsAssignableFrom(queryType))
            {
                throw MediatorServerException.CreateForNonContractType(queryType);
            }
            var query = _serializer.DeserializeRequest(request);
            if (query == null)
            {
                throw new MediatorServerException($"Can not deserialize contract as type {request.ObjectName}");
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
            var resultType = RequestGenericHelpers.GetRequestResultType(query.GetType());
            if (resultType == null)
            {
                throw new MediatorServerException($"Object {query.GetType()} is not assignable to type {typeof(IMediatorAction<>)}");
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
                return _serializer.SerializeResponse(mediatorResponse);
            }
            return SerializeError($"Unexpected result type from mediator pipeline. Was expected {typeof(MediatorResponse)} but {result?.GetType()} was returned instead.");
        }

        private string SerializeError(string errorMessage)
        {
            return _serializer.SerializeResponse(new MediatorResponse(errorMessage));
        }
    }
}
