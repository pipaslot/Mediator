using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization.V3.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Serialization.V3;
// TODO get rid of the V3 namespace

internal class JsonContractSerializer : IContractSerializer
{
    private readonly JsonSerializerOptions _defaultOptions;
    private readonly ICredibleProvider _credibleProvider;
    private readonly IMediatorOptions _mediatorOptions;

    public JsonContractSerializer(ICredibleProvider credibleProvider, IMediatorOptions mediatorOptions)
    {
        _credibleProvider = credibleProvider;
        _mediatorOptions = mediatorOptions;
        _defaultOptions = CreateOptions();
    }

    private JsonSerializerOptions CreateOptions() => new()
    {
        IgnoreReadOnlyProperties = _mediatorOptions.IgnoreReadOnlyProperties,
        PropertyNamingPolicy = null,
        Converters =
        {
            new InterfaceConverter<IMediatorAction>(_credibleProvider),
            new MediatorResponseConverter(_credibleProvider),
            new InterfaceConverterFactory(_credibleProvider)
        }
    };


    public SerializedRequest SerializeRequest(IMediatorAction request)
    {
        var converter = new StreamExtractingConverter();
        var options = CreateOptions();
        options.Converters.Add(converter);

        var json = JsonSerializer.Serialize(request, typeof(IMediatorAction), options);
        return new SerializedRequest(json, converter.GetStreams());
    }

    public async ValueTask<IMediatorAction> DeserializeRequest(Stream action, ICollection<StreamContract> dataStreams)
    {
        try
        {
            var options = CreateOptions();
            options.Converters.Add(new StreamExtractingConverter(dataStreams));
            var contract = await JsonSerializer.DeserializeAsync<IMediatorAction>(action, options).ConfigureAwait(false);
            if (contract != null)
            {
                return contract;
            }
        }
        catch (Exception e) when (e is not MediatorException && e is not MediatorHttpException)
        {
            throw MediatorHttpException.CreateForInvalidRequest(e);
        }

        throw MediatorHttpException.CreateForInvalidRequest();
    }

    public string SerializeResponse(IMediatorResponse response)
    {
        return JsonSerializer.Serialize(response, _defaultOptions);
    }

    public IMediatorResponse<TResult> DeserializeResponse<TResult>(string response)
    {
        if (!string.IsNullOrWhiteSpace(response))
        {
            try
            {
                var serializedResult = JsonSerializer.Deserialize<IMediatorResponse>(response, _defaultOptions);
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