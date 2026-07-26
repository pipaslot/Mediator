namespace Pipaslot.Mediator.Http;

/// <summary>
/// Marker added to the root action's <see cref="Pipaslot.Mediator.Middlewares.MediatorContext.Results"/> via
/// <see cref="MediatorContextExtensions.SetResponseStatusCodeHint"/> to let a middleware or handler influence the
/// HTTP status code of the root response without touching <c>HttpContext</c> directly.
/// <para>
/// Only read by <see cref="MediatorMiddleware"/>, from the root action's Results, and is always stripped out
/// before the default JSON response body is serialized - it never appears on the wire.
/// </para>
/// <para>
/// Like <see cref="IMediatorHttpResult"/>, a hint added on a nested <see cref="Pipaslot.Mediator.Middlewares.MediatorContext"/>
/// stays trapped in that context's own Results and is discarded once the nested call returns - it never leaks into
/// the root response unless the calling handler explicitly forwards it, which is not a supported pattern here.
/// </para>
/// </summary>
internal sealed class ResponseStatusCodeHint(int statusCode)
{
    public int StatusCode { get; } = statusCode;
}
