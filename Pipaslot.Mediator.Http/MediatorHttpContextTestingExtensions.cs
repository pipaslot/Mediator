using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Http.Internal;

namespace Pipaslot.Mediator.Http;

/// <summary>
/// Lets a test simulate an incoming public API request without going through a real ASP.NET Core HTTP pipeline.
/// </summary>
public static class MediatorHttpContextTestingExtensions
{
    /// <summary>
    /// Marks <paramref name="context"/> as if it had already passed through <see cref="MediatorMiddleware"/>, so
    /// that gating middlewares registered via <see cref="MiddlewareRegistratorExtensions.UseWhenDirectHttpCall"/>
    /// (including <see cref="MiddlewareRegistratorExtensions.UseAuthorizationWhenDirectHttpCall"/>) apply to a call
    /// dispatched while this <see cref="HttpContext"/> is the ambient one - even though no real HTTP request ran.
    /// <para>
    /// Use this in a test that hosts the mediator and calls <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute"/>
    /// (or their <c>Unhandled</c> counterparts) directly, without a <c>TestServer</c>/<c>WebApplicationFactory</c>, but
    /// still needs direct-call-only middlewares to be enforced. Assign the marked context to a test double of
    /// <see cref="IHttpContextAccessor"/> registered in the mediator's service provider:
    /// </para>
    /// <code>
    /// var httpContext = new DefaultHttpContext();
    /// httpContext.MarkAsMediatorPublicApiRequest();
    /// services.AddSingleton&lt;IHttpContextAccessor&gt;(new HttpContextAccessor { HttpContext = httpContext });
    /// </code>
    /// </summary>
    public static void MarkAsMediatorPublicApiRequest(this HttpContext context)
    {
        context.Features.Set(MediatorHttpContextFeature.Instance);
    }
}
