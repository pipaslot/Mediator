namespace Pipaslot.Mediator.Http.Internal;

/// <summary>
/// Marker <c>Instance</c> is set on <c>HttpContext.Features</c> by <see cref="Pipaslot.Mediator.Http.MediatorMiddleware"/>
/// as soon as it runs in the ASP.NET Core pipeline - unconditionally, regardless of whether the request actually
/// targets the mediator's own endpoint. Its presence marks the ambient <c>HttpContext</c> as belonging to a real,
/// currently in-flight incoming HTTP request that has reached the mediator middleware, as opposed to a background
/// service (or other out-of-request code) holding a stale/ambient <c>HttpContext</c>, or no HTTP context at all.
/// <para>
/// On its own this only proves the middleware ran somewhere in the current request; it says nothing about which
/// <see cref="IMediator"/> call is being made right now.
/// <see cref="HttpContextAccessorExtensions.IsExecutedFromPublicApi(Microsoft.AspNetCore.Http.IHttpContextAccessor, IMediatorContextAccessor)"/>
/// combines it with <c>IMediatorContextAccessor.IsFirstAction()</c> to tell a direct call - the outermost
/// <see cref="IMediator"/> call in a request that originated from the application's public API (the mediator's own
/// HTTP endpoint, a controller, a minimal API...) - apart from a nested call (a handler calling
/// <see cref="IMediator"/> again for another action), which must not be treated as a direct HTTP call even though
/// the same <c>HttpContext</c> is still ambient.
/// </para>
/// </summary>
internal class MediatorHttpContextFeature
{
    internal static MediatorHttpContextFeature Instance { get; } = new();
}