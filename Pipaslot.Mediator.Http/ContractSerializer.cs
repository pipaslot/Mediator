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

        public string SerializeRequest(object request, out string actionName)
        {
            actionName = request.GetType().AssemblyQualifiedName;
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
                queryType = Type.GetType(GetTypeWithoutAssembly(serializedResult.ObjectName));
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

        /// <summary>
        /// This method converst type Definition like "System.Collections.Generic.List`1[[MyType, MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=none"
        /// to "System.Collections.Generic.List`1[[MyType, MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"
        /// This is fix providing backward compatibility for .NET Core 3.1 and older where was issue with parsing types belonging to assembly System.Private.CoreLib, because this assembly is not available or the version might be different
        /// </summary>
        /// <param name="fullTypeAsString"></param>
        /// <returns></returns>
        internal static string GetTypeWithoutAssembly(string fullTypeAsString)
        {
            var isGeneric = fullTypeAsString.Contains("]]");
            if (isGeneric)
            {
                var startIndex = fullTypeAsString.IndexOf("[[") + 2;
                var endIndex = fullTypeAsString.LastIndexOf("]]");
                var before = fullTypeAsString.Substring(0, startIndex);
                var between = fullTypeAsString.Substring(startIndex, endIndex - startIndex);
                var after = fullTypeAsString.Substring(endIndex);
                return before + GetTypeWithoutAssembly(between) + RemoveAssemblySuffix(after);
            }
            else
            {
                return RemoveAssemblySuffix(fullTypeAsString);
            }
        }

        private static string RemoveAssemblySuffix(string typeAsString)
        {
            var assemblyIndex = typeAsString.LastIndexOf(", System.Private.CoreLib");

            return assemblyIndex >= 0
                ? typeAsString.Substring(0, assemblyIndex)
                : typeAsString;
        }

    }
}
