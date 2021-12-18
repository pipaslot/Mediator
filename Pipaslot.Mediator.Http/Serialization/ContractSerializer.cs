using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Serialization
{
    internal class ContractSerializer : IContractSerializer
    {
        public ContractSerializer(ICredibleActionProvider credibleActions)
        {
            _credibleActions = credibleActions;
        }

        private readonly static JsonSerializerOptions _serializationOptions = new()
        {
            PropertyNamingPolicy = null
        };
        private readonly ICredibleActionProvider _credibleActions;

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

        public IMediatorAction DeserializeRequest(string requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                throw MediatorHttpException.CreateForEmptyBody();
            }
            var contract = JsonSerializer.Deserialize<MediatorRequestSerializable>(requestBody);
            if (contract == null)
            {
                throw MediatorHttpException.CreateForUnparsedContract();
            }
            var actionType = Type.GetType(contract.ObjectName);
            if (actionType == null)
            {
                throw MediatorHttpException.CreateForUnrecognizedType(contract.ObjectName);
            }
            _credibleActions.VerifyCredibility(actionType);
            
            var content = JsonSerializer.Deserialize(contract.Json, actionType);
            if (content == null)
            {
                throw MediatorHttpException.CreateForUnparsedContract();
            }
            return (IMediatorAction)content;
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
            if (serializedResult == null)
            {
                throw new Exception("Can not deserialize server response. Please check if Pipaslot.Mediator.Client and Pipaslot.Mediator.Server have the same version or if response is valid JSON.");
            }

            return new ResponseDeserialized<TResult>
            {
                Success = serializedResult.Success,
                ErrorMessages = serializedResult.ErrorMessages,
                Results = serializedResult.Results.Select(r => DeserializeResult(r)).ToArray()
            };
        }

        private object DeserializeResult(MediatorResponseSerializable.SerializedResult serializedResult)
        {
            var queryType = ContractSerializerTypeHelper.GetType(serializedResult.ObjectName);
            var result = JsonSerializer.Deserialize(serializedResult.Json, queryType);
            if (result == null)
            {
                throw new Exception($"Can not deserialize contract as type {serializedResult.ObjectName} received from server");
            }
            return result;
        }

        #region Contracts

        /// <summary>
        /// Request contract with all necessary fields sent over network
        /// </summary>
        internal class MediatorRequestSerializable
        {
            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
            public string Json { get; set; } = string.Empty;

            // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
            public string ObjectName { get; set; } = string.Empty;
        }

        /// <summary>
        /// Response contract with all necessary fields sent over network
        /// </summary>
        internal class MediatorResponseSerializable
        {
            public bool Success { get; set; }
            public SerializedResult[] Results { get; set; } = new SerializedResult[0];
            public string[] ErrorMessages { get; set; } = new string[0];

            public class SerializedResult
            {
                // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
                public string Json { get; set; } = string.Empty;

                // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
                public string ObjectName { get; set; } = string.Empty;
            }
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
