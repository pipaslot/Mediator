using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Http.Internal;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests.E2E;

/// <summary>
/// Covers <see cref="MiddlewareRegistratorExtensions.UseAuthorizationWhenDirectHttpCall"/>: unlike
/// <see cref="DirectHttpCallGatingExtensionsTests"/>, which only proves a probe middleware runs or doesn't, this
/// class exercises the real business contract - that <see cref="AuthorizationMiddleware"/> actually denies a direct
/// call and is bypassed for a nested one, via genuine <see cref="IMediator.Dispatch"/> success/failure. The
/// direct-vs-nested condition itself is not re-verified here (see <c>Internal.HttpContextAccessorExtensionsTests</c>
/// and <see cref="DirectHttpCallGatingExtensionsTests"/>).
/// </summary>
public class AuthorizationWhenDirectHttpCallTests
{
    [Fact]
    public async Task DirectCallWithoutRequiredRole_IsDeniedAndHandlerNeverRuns()
    {
        var (mediator, accessor) = CreateAuthorizedMediator();
        accessor.Principal = CreatePrincipal("Guest");

        var result = await mediator.Dispatch(new SecuredAction());

        Assert.False(result.Success);
    }

    [Fact]
    public async Task NestedCallWithoutRequiredRole_BypassesAuthorizationAndHandlerRuns()
    {
        // Only the first (public API) action in the chain is gated - a nested dispatch of the same secured action
        // is not itself "direct", so it must bypass the check.
        var (mediator, accessor) = CreateAuthorizedMediator();
        accessor.Principal = CreatePrincipal("Guest");

        var result = await mediator.Dispatch(new RootDelegatingToSecuredAction());

        Assert.True(result.Success);
    }

    private static (IMediator Mediator, StubClaimPrincipalAccessor Accessor) CreateAuthorizedMediator()
    {
        var accessor = new StubClaimPrincipalAccessor();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(CreateHttpContextAccessor(indicatePublicApiAccess: true));
        services.AddMediator()
            .AddActions([typeof(SecuredAction), typeof(RootDelegatingToSecuredAction)])
            .AddHandlers([typeof(SecuredActionHandler), typeof(RootDelegatingToSecuredActionHandler)])
            .UseAuthorizationWhenDirectHttpCall();
        services.AddSingleton<IClaimPrincipalAccessor>(accessor);
        var provider = services.BuildServiceProvider();
        return (provider.GetRequiredService<IMediator>(), accessor);
    }

    private static ClaimsPrincipal CreatePrincipal(string role)
    {
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Role, role)], authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(bool indicatePublicApiAccess)
    {
        var features = new FeatureCollection();
        if (indicatePublicApiAccess)
        {
            features.Set(MediatorHttpContextFeature.Instance);
        }

        var context = Substitute.For<HttpContext>();
        context.Features.Returns(features);

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return accessor;
    }

    public class SecuredAction : IMessage;

    public class SecuredActionHandler : IMediatorHandler<SecuredAction>, IHandlerAuthorization<SecuredAction>
    {
        public IPolicy Authorize(SecuredAction action) => IdentityPolicy.Role("Admin");

        public Task Handle(SecuredAction action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    public class RootDelegatingToSecuredAction : IMessage;

    [AnonymousPolicy]
    public class RootDelegatingToSecuredActionHandler(IMediator mediator) : IMediatorHandler<RootDelegatingToSecuredAction>
    {
        public Task Handle(RootDelegatingToSecuredAction action, CancellationToken cancellationToken)
        {
            return mediator.DispatchUnhandled(new SecuredAction(), cancellationToken);
        }
    }

    private class StubClaimPrincipalAccessor : IClaimPrincipalAccessor
    {
        public ClaimsPrincipal? Principal { get; set; }
    }
}
