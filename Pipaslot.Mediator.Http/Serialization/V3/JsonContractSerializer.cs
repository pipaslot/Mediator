using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.V3.Converters;
using System;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Serialization.V3;

internal class JsonContractSerializer(ICredibleProvider credibleProvider, IMediatorOptions mediatorOptions) : IContractSerializer
{
    private readonly JsonSerializerOptions _serializationOptions = new()
    {
        IgnoreReadOnlyProperties = mediatorOptions.IgnoreReadOnlyProperties,
        PropertyNamingPolicy = null,
        Converters =
        {
            new InterfaceConverter<IMediatorAction>(credibleProvider),
            new MediatorResponseConverter(credibleProvider),
            new InterfaceConverterFactory(credibleProvider)
        }
    };
    internal static readonly JsonSerializerOptions SerializationOptionsWithoutConverters = new() { PropertyNamingPolicy = null };

    public string SerializeRequest(IMediatorAction request)
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
            catch (Exception e) when (e is not MediatorException && e is not MediatorHttpException)
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
                    return new MediatorResponse<TResult>(serializedResult.Success, serializedResult.Results);
                }
            }
            catch (Exception e) when (e is not MediatorException && e is not MediatorHttpException)
            {
                throw MediatorHttpException.CreateForInvalidResponse(response, e);
            }
        }

        throw MediatorHttpException.CreateForInvalidResponse(response);
    }
}