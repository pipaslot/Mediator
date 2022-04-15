using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.Converters;
using Pipaslot.Mediator.Http.Serialization.Models;
using System;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Serialization
{
    internal class FullJsonContractSerializer : IContractSerializer
    {
        private readonly JsonSerializerOptions _serializationOptions;
        internal readonly static JsonSerializerOptions SerializationOptionsWithoutConverters = new()
        {
            PropertyNamingPolicy = null
        };

        public FullJsonContractSerializer(ICredibleActionProvider credibleActions, ICredibleResultProvider credibleResults)
        {
            _serializationOptions = new()
            {
                PropertyNamingPolicy = null,
                Converters =
                {
                    new ContractSerializableConverter(credibleActions),
                    new ResponseDeserializedConverter(credibleResults)
                }
            };
        }

        public string SerializeRequest(object request)
        {
            var actionName = request.GetType().AssemblyQualifiedName;
            var contract = new ContractSerializable(request, actionName);
            return JsonSerializer.Serialize(contract, typeof(ContractSerializable), _serializationOptions);
        }

        public IMediatorAction DeserializeRequest(string body)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var contract = JsonSerializer.Deserialize<ContractSerializable>(body, _serializationOptions);

                    if (contract != null)
                    {
                        return (IMediatorAction)contract.Content;
                    }
                }
                catch (MediatorException)
                {
                    throw;
                }
                catch (MediatorHttpException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    throw MediatorHttpException.CreateForInvalidRequest(body, e);
                }
            }
            throw MediatorHttpException.CreateForInvalidRequest(body);
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
            if (!string.IsNullOrWhiteSpace(response))
            {
                try
                {
                    var serializedResult = JsonSerializer.Deserialize<ResponseDeserialized>(response, _serializationOptions);
                    if (serializedResult != null)
                    {
                        return new ResponseDeserialized<TResult>
                        {
                            Success = serializedResult.Success,
                            ErrorMessages = serializedResult.ErrorMessages,
                            Results = serializedResult.Results
                        };
                    }
                }
                catch (Exception e)
                {
                    throw MediatorHttpException.CreateForInvalidResponse(response, e);
                }
            }
            throw MediatorHttpException.CreateForInvalidResponse(response);
        }
    }
}
