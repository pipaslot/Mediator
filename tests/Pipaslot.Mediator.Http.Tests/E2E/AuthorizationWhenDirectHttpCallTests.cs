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
/// <para>
/// Most tests here fake the direct-call condition via the <c>InternalsVisibleTo</c>-only
/// <see cref="MediatorHttpContextFeature"/>, mirroring how a real HTTP request looks from inside this assembly.
/// <see cref="SimulatedPublicApiCallViaTestingExtension_WithoutRequiredRole_ExecuteUnhandledThrows"/> instead uses
/// only <see cref="MediatorHttpContextTestingExtensions.MarkAsMediatorPublicApiRequest"/>, the public surface a
/// consumer project (without internal access) would use for the same simulation in its own tests.
/// </para>
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

    [Fact]
    public async Task SimulatedPublicApiCallViaTestingExtension_WithoutRequiredRole_ExecuteUnhandledThrows()
    {
        // Reproduces a host that never runs a real HTTP request (e.g. a unit test calling ExecuteUnhandled
        // directly): MarkAsMediatorPublicApiRequest is the supported public replacement for the InternalsVisibleTo
        // trick used by CreateHttpContextAccessor above, so a consumer project without internal access can still
        // make UseAuthorizationWhenDirectHttpCall trigger in its own tests.
        var accessor = new StubClaimPrincipalAccessor { Principal = CreatePrincipal("Guest") };
        var services = new ServiceCollection();
        services.AddLogging();
        var httpContext = Substitute.For<HttpContext>();
        httpContext.Features.Returns(new FeatureCollection());
        httpContext.MarkAsMediatorPublicApiRequest();
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);
        services.AddSingleton(httpContextAccessor);
        services.AddMediator()
            .AddActions([typeof(SecuredRequest)])
            .AddHandlers([typeof(SecuredRequestHandler)])
            .UseAuthorizationWhenDirectHttpCall();
        services.AddSingleton<IClaimPrincipalAccessor>(accessor);
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<AuthorizationRuleNotMetException>(() => mediator.ExecuteUnhandled(new SecuredRequest()));
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

    public class SecuredRequest : IRequest<string>;

    public class SecuredRequestHandler : IMediatorHandler<SecuredRequest, string>, IHandlerAuthorization<SecuredRequest>
    {
        public IPolicy Authorize(SecuredRequest action) => IdentityPolicy.Role("Admin");

        public Task<string> Handle(SecuredRequest action, CancellationToken cancellationToken)
        {
            return Task.FromResult("ok");
        }
    }

    private class StubClaimPrincipalAccessor : IClaimPrincipalAccessor
    {
        public ClaimsPrincipal? Principal { get; set; }
    }
}
