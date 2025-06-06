using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System.Net;

namespace Pipaslot.Mediator.Http;

public class ServerMediatorUrlFormatter(
    ServerMediatorOptions options,
    IContractSerializer serializer) : IMediatorUrlFormatter
{
    public string FormatHttpGet(IMediatorAction action)
    {
        var serialized = serializer.SerializeRequest(action).Json;
        var decoded = WebUtility.UrlDecode(serialized);
        return $"{options.Endpoint}?{MediatorConstants.ActionQueryParamName}={decoded}";
    }
}