using System;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Serialization
{
    public interface IContractSerializer
    {
        MediatorRequestSerializable CreateContract(object request);

        string SerializeResponse(MediatorResponse response);

        object? DeserializeRequest(MediatorRequestSerializable request);
        string SerializeRequest(MediatorRequestSerializable contract);

        IMediatorResponse<TResult> DeserializeResults<TResult>(MediatorResponseSerializableV2 serializedResult);
    }

    public class ContractSerializerV1 : IContractSerializer
    {
        private readonly static JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };
        public MediatorRequestSerializable CreateContract(object request)
        {
            return new MediatorRequestSerializable
            {
                Json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                ObjectName = request.GetType().AssemblyQualifiedName
            };
        }

        public object? DeserializeRequest(MediatorRequestSerializable request)
        {
            var queryType = Type.GetType(request.ObjectName);
            return JsonSerializer.Deserialize(request.Json, queryType);
        }

        public IMediatorResponse<TResult> DeserializeResults<TResult>(MediatorResponseSerializableV2 serializedResult)
        {
            throw new NotImplementedException("Not not supported due to breaking compatibility. Serializer Version 1 is not supported. Please update your Pipaslot.Mediator.Client to version 2.0.0 or higher");
        }

        public string SerializeRequest(MediatorRequestSerializable contract)
        {
            return JsonSerializer.Serialize(contract, typeof(MediatorRequestSerializable), _serializationOptions);
        }

        public string SerializeResponse(MediatorResponse response)
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

    public class ContractSerializerV2 : IContractSerializer
    {
        private readonly static JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };

        public MediatorRequestSerializable CreateContract(object request)
        {
            return new MediatorRequestSerializable
            {
                Json = JsonSerializer.Serialize(request, _serializationOptions),
                ObjectName = request.GetType().AssemblyQualifiedName
            };
        }

        public string SerializeResponse(MediatorResponse response)
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

        private MediatorResponseSerializableV2.SerializedResult SerializerResult(object request)
        {
            return new MediatorResponseSerializableV2.SerializedResult
            {
                Json = JsonSerializer.Serialize(request, _serializationOptions),
                ObjectName = request.GetType().AssemblyQualifiedName
            };
        }

        public object? DeserializeRequest(MediatorRequestSerializable request)
        {
            var queryType = Type.GetType(request.ObjectName);
            return JsonSerializer.Deserialize(request.Json, queryType);
        }
        public string SerializeRequest(MediatorRequestSerializable contract)
        {
            return JsonSerializer.Serialize(contract, typeof(MediatorRequestSerializable), _serializationOptions);
        }

        public IMediatorResponse<TResult> DeserializeResults<TResult>(MediatorResponseSerializableV2 serializedResult)
        {
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

        private object DeserializeResult(MediatorResponseSerializableV2.SerializedResult serializedResult)
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
