using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System.Net;

namespace Pipaslot.Mediator.Http;

/// <summary>
/// Server-side <see cref="IMediatorUrlFormatter"/> building the endpoint URL with the serialized action in the query string.
/// </summary>
/// <remarks>
/// Registered by <c>AddMediatorServer</c>. A URL built this way is only served when
/// <see cref="ServerMediatorOptions.HttpGetConditions"/> allows the action over HTTP GET.
/// </remarks>
/// <param name="options">Server options providing the endpoint.</param>
/// <param name="serializer">Serializer of the action placed into the URL.</param>
public class ServerMediatorUrlFormatter(
    ServerMediatorOptions options,
    IContractSerializer serializer) : IMediatorUrlFormatter
{
    /// <inheritdoc/>
    public string FormatHttpGet(IMediatorAction action)
    {
        var serialized = serializer.SerializeRequest(action).Json;
        var decoded = WebUtility.UrlDecode(serialized);
        return $"{options.Endpoint}?{MediatorConstants.ActionQueryParamName}={decoded}";
    }
}