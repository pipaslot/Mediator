using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.V3.Converters;
using System;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Serialization.V3
{
    internal class JsonContractSerializer : IContractSerializer
    {
        private readonly JsonSerializerOptions _serializationOptions;
        internal readonly static JsonSerializerOptions SerializationOptionsWithoutConverters = new()
        {
            PropertyNamingPolicy = null
        };

        public JsonContractSerializer(ICredibleProvider credibleProvider)
        {
            _serializationOptions = new()
            {
                PropertyNamingPolicy = null,
                Converters =
                {
                    new JsonInterfaceConverter<IMediatorAction>(credibleProvider),
                    new MediatorResponseConverter(credibleProvider)
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
            return JsonSerializer.Serialize(response, _serializationOptions);
        }

        public IMediatorResponse<TResult> DeserializeResponse<TResult>(string response)
        {
            if (!string.IsNullOrWhiteSpace(response))
            {
                try
                {
                    var serializedResult = JsonSerializer.Deserialize<IMediatorResponse>(response, _serializationOptions);
                    if (serializedResult != null)
                    {
                        return new MediatorResponse<TResult>(serializedResult.Success, serializedResult.Results, serializedResult.ErrorMessages);
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
