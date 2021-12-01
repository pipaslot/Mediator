using Pipaslot.Mediator.Http.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Http
{
    public class FullJsonContractSerializer : IContractSerializer
    {
        private readonly static JsonSerializerOptions _serializationOptions = new()
        {
            PropertyNamingPolicy = null,
            Converters =
            {
                new ContractSerializableConverter(),
                new ResponseDeserializedConverter()
            }
        };
        internal readonly static JsonSerializerOptions SerializationOptionsWithoutConverters = new()
        {
            PropertyNamingPolicy = null
        };

        public string SerializeRequest(object request)
        {
            var actionName = request.GetType().AssemblyQualifiedName;
            var contract = new ContractSerializable(request, actionName);
            return JsonSerializer.Serialize(contract, typeof(ContractSerializable), _serializationOptions);
        }

        public MediatorRequestDeserialized DeserializeRequest(string requestBody)
        {
            var contract = JsonSerializer.Deserialize<ContractSerializable>(requestBody, _serializationOptions);
            if (contract == null)
            {
                return new MediatorRequestDeserialized(null, null, null);
            }
            if (contract.Content == null)
            {
                return new MediatorRequestDeserialized(null, null, contract.Type);
            }
            return new MediatorRequestDeserialized(contract.Content, contract.Content.GetType(), contract.Type);
        }

        public string SerializeResponse(IMediatorResponse response)
        {
            var obj = new ResponseSerializable
            {
                ErrorMessages = response.ErrorMessages.ToArray(),
                Results = response.Results
                    .Select(request => new ContractSerializable(request, request.GetType().AssemblyQualifiedName))
                    .ToArray(),
                Success = response.Success
            };
            return JsonSerializer.Serialize(obj, _serializationOptions);
        }

        public IMediatorResponse<TResult> DeserializeResponse<TResult>(string response)
        {
            var serializedResult = JsonSerializer.Deserialize<ResponseDeserialized>(response, _serializationOptions);
            if (serializedResult == null)
            {
                throw new Exception("Can not deserialize server response. Please check if Pipaslot.Mediator.Client and Pipaslot.Mediator.Server have the same version or if response is valid JSON.");
            }
            return new ResponseDeserialized<TResult>
            {
                Success = serializedResult.Success,
                ErrorMessages = serializedResult.ErrorMessages,
                Results = serializedResult.Results
            };
        }

        #region Contracts
        internal class ContractSerializable
        {
            public ContractSerializable(object content, string type)
            {
                Content = content;
                Type = type;
            }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
            public object Content { get; }

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
            public string Type { get; }
        }

        internal class ResponseSerializable
        {
            public bool Success { get; set; }
            public ContractSerializable[] Results { get; set; } = new ContractSerializable[0];
            public string[] ErrorMessages { get; set; } = new string[0];
        }

        internal class ResponseDeserialized
        {
            public bool Success { get; set; }
            public object[] Results { get; set; } = new object[0];
            public string[] ErrorMessages { get; set; } = new string[0];
        }

        internal class ResponseDeserialized<TResult> : IMediatorResponse<TResult>
        {
            public bool Success { get; set; }
            public bool Failure => !Success;
            public string ErrorMessage => string.Join(";", ErrorMessages);
            public TResult Result => (TResult)Results.FirstOrDefault(r => r is TResult);
            public object[] Results { get; set; } = new object[0];
            public string[] ErrorMessages { get; set; } = new string[0];
        }

        #endregion

    }
}
