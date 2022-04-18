using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.Converters;
using Pipaslot.Mediator.Http.Serialization.Models;
using System;
using System.Linq;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Serialization
{
    internal class SimpleJsonContractSerializer : IContractSerializer
    {
        private readonly JsonSerializerOptions _serializationOptions;
        internal readonly static JsonSerializerOptions SerializationOptionsWithoutConverters = new()
        {
            PropertyNamingPolicy = null
        };

        public SimpleJsonContractSerializer(ICredibleActionProvider credibleActions, ICredibleResultProvider credibleResults)
        {
            _serializationOptions = new()
            {
                PropertyNamingPolicy = null,
                Converters =
                {
                    new JsonInterfaceConverter<IMediatorAction>(credibleActions),
                    new SimpleContractSerializableConverter(credibleActions),
                    new SimpleResponseDeserializedConverter(credibleResults)
                }
            };
        }

        public string SerializeRequest(object request)
        {
            return JsonSerializer.Serialize(request, typeof(IMediatorAction), _serializationOptions);
        }

        public IMediatorAction DeserializeRequest(string body)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    var contract = JsonSerializer.Deserialize<IMediatorAction>(body, _serializationOptions);
                    if (contract != null)
                    {
                        return contract;
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
                    .Select(result => new ContractSerializable(result, ContractSerializerTypeHelper.GetIdentifier(result.GetType())))
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
