using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.Authorization;

/// <summary>
/// Exercises <see cref="AuthorizationMiddleware"/> through a real <see cref="IMediator"/> call.
/// <see cref="Pipaslot.Mediator.Tests.Authorization.PolicyResolverTests"/> covers <see cref="PolicyResolver"/> in
/// isolation with handlers that are never actually resolved from a container or invoked by a pipeline; this class
/// verifies the middleware wires that resolver into the real Dispatch/Execute path - that a denied policy prevents
/// the handler from running, and that a handler declaring no policy at all is denied by default rather than
/// implicitly allowed.
/// </summary>
public class AuthorizationTests
{
    [Fact]
    public async Task Dispatch_PrincipalHasRequiredRole_HandlerExecutesAndSucceeds()
    {
        var (sut, accessor) = CreateMediator();
        accessor.Principal = CreatePrincipal(ClaimTypes.Role, "Admin");

        var result = await sut.Dispatch(new SecuredMessage());

        Assert.True(result.Success);
    }

    [Fact]
    public async Task Dispatch_PrincipalMissingRequiredRole_ReturnsFailureWithoutRunningHandler()
    {
        var (sut, accessor) = CreateMediator();
        accessor.Principal = CreatePrincipal(ClaimTypes.Role, "Guest");

        var result = await sut.Dispatch(new SecuredMessage());

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DispatchUnhandled_PrincipalMissingRequiredRole_ThrowsAuthorizationRuleNotMetException()
    {
        var (sut, accessor) = CreateMediator();
        accessor.Principal = CreatePrincipal(ClaimTypes.Role, "Guest");

        var ex = await Assert.ThrowsAsync<AuthorizationRuleNotMetException>(() => sut.DispatchUnhandled(new SecuredMessage()));

        Assert.Equal(AuthorizationExceptionTypes.RuleNotMet, ex.Type);
    }

    [Fact]
    public async Task Dispatch_NeitherActionNorHandlerDeclaresAuthorizationPolicy_ReturnsFailureWithoutRunningHandler()
    {
        // AuthorizationMiddleware denies by default: a policy can come from either the action (IActionAuthorization,
        // a policy attribute on the action type) or the handler (IHandlerAuthorization/Async, a policy attribute on
        // the handler type). Only when NEITHER side declares one at all is the call implicitly denied.
        var sut = CreateMediatorForUnsecuredMessage();

        var result = await sut.Dispatch(new UnsecuredMessage());

        Assert.False(result.Success);
    }

    [Fact]
    public async Task DispatchUnhandled_NeitherActionNorHandlerDeclaresAuthorizationPolicy_ThrowsNoAuthorizationException()
    {
        var sut = CreateMediatorForUnsecuredMessage();

        var ex = await Assert.ThrowsAsync<AuthorizationException>(() => sut.DispatchUnhandled(new UnsecuredMessage()));

        Assert.Equal(AuthorizationExceptionTypes.NoAuthorization, ex.Type);
    }

    private static IMediator CreateMediatorForUnsecuredMessage()
    {
        var services = Factory.CreateServiceProvider(c => c
            .AddActions([typeof(UnsecuredMessage)])
            .AddHandlers([typeof(UnsecuredMessageHandler)])
            .UseAuthorization());
        return services.GetRequiredService<IMediator>();
    }

    private static (IMediator Mediator, StubClaimPrincipalAccessor Accessor) CreateMediator()
    {
        var accessor = new StubClaimPrincipalAccessor();
        var services = Factory.CreateServiceProvider((c, sc) =>
        {
            c.AddActions([typeof(SecuredMessage)]);
            c.AddHandlers([typeof(SecuredMessageHandler)]);
            c.UseAuthorization();
            sc.AddSingleton<IClaimPrincipalAccessor>(accessor);
        });
        return (services.GetRequiredService<IMediator>(), accessor);
    }

    private static ClaimsPrincipal CreatePrincipal(string claimType, string claimValue)
    {
        var identity = new ClaimsIdentity([new Claim(claimType, claimValue)], authenticationType: "Test");
        return new ClaimsPrincipal(identity);
    }

    public class SecuredMessage : IMessage;

    public class SecuredMessageHandler : IMediatorHandler<SecuredMessage>, IHandlerAuthorization<SecuredMessage>
    {
        public IPolicy Authorize(SecuredMessage action) => IdentityPolicy.Role("Admin");

        public Task Handle(SecuredMessage action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private class StubClaimPrincipalAccessor : IClaimPrincipalAccessor
    {
        public ClaimsPrincipal? Principal { get; set; }
    }

    public class UnsecuredMessage : IMessage;

    public class UnsecuredMessageHandler : IMediatorHandler<UnsecuredMessage>
    {
        public Task Handle(UnsecuredMessage action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
