using Pipaslot.Mediator.Http.Contracts;
using System;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Http
{
    public class ContractSerializer : IContractSerializer
    {
        private readonly static JsonSerializerOptions _serializationOptions = new()
        {
            PropertyNamingPolicy = null
        };

        public string SerializeRequest(object request)
        {
            var actionName = request.GetType().AssemblyQualifiedName;
            var contract = new MediatorRequestSerializable
            {
                Json = JsonSerializer.Serialize(request, _serializationOptions),
                ObjectName = actionName
            };
            return JsonSerializer.Serialize(contract, typeof(MediatorRequestSerializable), _serializationOptions);
        }

        public MediatorRequestDeserialized DeserializeRequest(string requestBody)
        {
            var contract = JsonSerializer.Deserialize<MediatorRequestSerializable>(requestBody);
            if(contract == null)
            {
                return new MediatorRequestDeserialized(null, null, null);
            }
            var actionType = Type.GetType(contract.ObjectName);
            if (actionType == null)
            {
                return new MediatorRequestDeserialized(null, null, contract.ObjectName);
            }
            var content = JsonSerializer.Deserialize(contract.Json, actionType);
            return new MediatorRequestDeserialized(content, actionType, contract.ObjectName);
        }

        public string SerializeResponse(IMediatorResponse response)
        {
            var obj = new MediatorResponseSerializable
            {
                ErrorMessages = response.ErrorMessages.ToArray(),
                Results = response.Results
                    .Select(r => SerializerResult(r))
                    .ToArray(),
                Success = response.Success
            };
            return JsonSerializer.Serialize(obj, _serializationOptions);
        }

        private MediatorResponseSerializable.SerializedResult SerializerResult(object request)
        {
            return new MediatorResponseSerializable.SerializedResult
            {
                Json = JsonSerializer.Serialize(request, _serializationOptions),
                ObjectName = request.GetType().AssemblyQualifiedName
            };
        }

        public IMediatorResponse<TResult> DeserializeResponse<TResult>(string response)
        {
            var serializedResult = JsonSerializer.Deserialize<MediatorResponseSerializable>(response);
            if(serializedResult == null)
            {
                throw new Exception("Can not deserialize server response. Please check if Pipaslot.Mediator.Client and Pipaslot.Mediator.Server have the same version or if response is valid JSON.");
            }
            var results = serializedResult.Results
                .Select(r => DeserializeResult(r))
                .ToArray();
            return new MediatorResponseDeserialized<TResult>
            {
                Success = serializedResult.Success,
                ErrorMessages = serializedResult.ErrorMessages,
                Results = serializedResult.Results.Select(r => DeserializeResult(r)).ToArray()
            };
        }

        private object DeserializeResult(MediatorResponseSerializable.SerializedResult serializedResult)
        {
            var queryType = Type.GetType(serializedResult.ObjectName);
            if (queryType == null)
            {
                queryType = Type.GetType(ContractSerializerTypeHelper.GetTypeWithoutAssembly(serializedResult.ObjectName));
                if (queryType == null)
                {
                    throw new Exception($"Can not recognize type {serializedResult.ObjectName} from received response. Ensure that type returned and serialized on server is available/referenced on client as well.");
                }
            }
            var result = JsonSerializer.Deserialize(serializedResult.Json, queryType);
            if (result == null)
            {
                throw new Exception($"Can not deserialize contract as type {serializedResult.ObjectName} received from server");
            }
            return result;
        }
    }
}
