namespace Pipaslot.Mediator.Http;

/// <summary>
/// Wire-level names and defaults that the client and the server of the HTTP transport must agree on.
/// </summary>
public class MediatorConstants
{
    /// <summary>
    /// Default URL path of the mediator endpoint, used when <see cref="Configuration.BaseMediatorOptions{TBuilder}.Endpoint"/> is not changed.
    /// </summary>
    public const string Endpoint = "/_mediator/request";

    /// <summary>
    /// Default HTTP status code of a response whose mediator result failed; see <see cref="Configuration.ServerMediatorOptions.ErrorHttpStatusCode"/>.
    /// </summary>
    public const int ErrorHttpStatusCode = 500;

    /// <summary>
    /// Name of the query-string parameter carrying the JSON-serialized action in an HTTP GET request.
    /// </summary>
    public const string ActionQueryParamName = "action";
    
    /// <summary>
    /// Name of the multipart form field holding the JSON-serialized action when the request also carries streams as files.
    /// </summary>
    public const string MultipartFormDataJson = "json";
}