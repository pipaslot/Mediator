using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Authorization;

/// <summary>
/// This tests simulate already resolved combination of single action and one or more actions. 
/// To make the tests simple, there are not connections between handlers and action like real usage.
/// </summary>
public class PolicyResolverTests
{
    private readonly IServiceProvider _services = Substitute.For<IServiceProvider>();

    [Fact]
    public async Task CheckPolicies_NoAuthorization_ThrowException()
    {
        await RunCheckPolicies(
            new NoAuthorization(),
            AuthorizationExceptionTypes.NoAuthorization);
    }

    [Fact]
    public async Task GetPolicies_SecuredSyncHandler_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new NoAuthorization(),
            1,
            new NoAuthorizationHandlerAuthorizationHandler());
    }

    [Fact]
    public async Task GetPolicies_SecuredAsyncHandler_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new NoAuthorization(),
            1,
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Fact]
    public async Task GetPolicies_SecuredAttrHandler_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new NoAuthorization(),
            1,
            new NoAuthorizationHandlerAttribute());
    }

    [Fact]
    public async Task GetPolicies_CombineSyncAndAsyncHandler_ResolveTwoPolicies()
    {
        await RunGetPolicies(
            new NoAuthorization(),
            2,
            new NoAuthorizationHandlerAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Fact]
    public async Task CheckPolicies_MultiHandlersButOneHandlerIsUnsecured_ThrowException()
    {
        await RunCheckPolicies(
            new NoAuthorization(),
            AuthorizationExceptionTypes.UnauthorizedHandler,
            new NoAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Fact]
    public async Task GetPolicies_AuthorizedActionByAttr_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new ActionAuthorizedByAttr(),
            1);
    }

    [Fact]
    public async Task GetPolicies_AuthorizedActionByInterface_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new ActionAuthorizedByInterface(),
            1);
    }

    [Fact]
    public async Task GetPolicies_CombineAllAvailablePolicies_ResolveTwoPolicies()
    {
        await RunGetPolicies(
            new ActionAuthorizedByAttrAndInterface(),
            5,
            new NoAuthorizationHandlerAttribute(),
            new NoAuthorizationHandlerAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Fact]
    public async Task GetPolicies_ReadPolicyFromInterfaces()
    {
        await RunGetPolicies(
            new AnonamousAction(),
            1);
    }


    [Fact]
    public async Task CheckPolicies_AuthorizedActionAndSyncAndAsyncHandlersAndUnauthorizedHandler_ThrowException()
    {
        await RunCheckPolicies(
            new ActionAuthorizedByAttr(),
            AuthorizationExceptionTypes.UnauthorizedHandler,
            new NoAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Fact]
    public async Task CheckPolicies_AllowedPolicy_DoesNotThrow()
    {
        var exception = await Record.ExceptionAsync(() =>
            PolicyResolver.CheckPolicies(_services, new ActionAuthorizedByAttr(), Array.Empty<object>(), CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task CheckPolicies_DeniedPolicy_ThrowsRuleNotMetException()
    {
        _services
            .GetService(typeof(INodeFormatter))
            .Returns(new DefaultNodeFormatter());

        var ex = await Assert.ThrowsAsync<AuthorizationRuleNotMetException>(() =>
            PolicyResolver.CheckPolicies(_services, new ActionRequiringRole(), Array.Empty<object>(), CancellationToken.None));

        Assert.Equal(AuthorizationExceptionTypes.RuleNotMet, ex.Type);
        Assert.Contains("Role 'Admin' is required.", ex.Message);
    }

    [Fact]
    public async Task GetPolicyRules_ResolvesPolicyIntoRuleSet()
    {
        var ruleSet = await PolicyResolver.GetPolicyRules(_services, new ActionAuthorizedByAttr(), Array.Empty<object>(), CancellationToken.None);

        var rule = Assert.Single(ruleSet.RulesRecursive);
        Assert.Equal(IdentityPolicy.AuthenticationPolicyName, rule.Name);
        Assert.Equal(IdentityPolicy.AnonymousValue, rule.Value);
        Assert.Equal(RuleOutcome.Allow, rule.Outcome);
    }

    [Fact]
    public async Task GetPolicies_NullHandlerInArray_IsSkipped()
    {
        var policies = await PolicyResolver.GetPolicies(new NoAuthorization(), new object?[] { null }, CancellationToken.None);

        Assert.Empty(policies);
    }

    [Fact]
    public async Task GetPolicies_SyncHandlerAuthorizeReturnsNull_ThrowsMediatorException()
    {
        var ex = await Assert.ThrowsAsync<MediatorException>(() =>
            PolicyResolver.GetPolicies(new NoAuthorization(), new object[] { new NullPolicySyncHandler() }, CancellationToken.None));

        Assert.Contains(typeof(NullPolicySyncHandler).FullName!, ex.Message);
    }

    [Fact]
    public async Task GetPolicies_AsyncHandlerAuthorizeReturnsNull_ThrowsMediatorException()
    {
        var ex = await Assert.ThrowsAsync<MediatorException>(() =>
            PolicyResolver.GetPolicies(new NoAuthorization(), new object[] { new NullPolicyAsyncHandler() }, CancellationToken.None));

        Assert.Contains(typeof(NullPolicyAsyncHandler).FullName!, ex.Message);
    }

    [Fact]
    public void HasActionPolicies_NoPoliciesAnywhere_ReturnsFalse()
    {
        var result = PolicyResolver.HasActionPolicies(typeof(NoAuthorization), Array.Empty<object>());

        Assert.False(result);
    }

    [Fact]
    public void HasActionPolicies_ActionHasAttribute_ReturnsTrue()
    {
        var result = PolicyResolver.HasActionPolicies(typeof(ActionAuthorizedByAttr), Array.Empty<object>());

        Assert.True(result);
    }

    [Fact]
    public void HasActionPolicies_HandlerHasAttribute_ReturnsTrue()
    {
        var result = PolicyResolver.HasActionPolicies(typeof(NoAuthorization), new object[] { new NoAuthorizationHandlerAttribute() });

        Assert.True(result);
    }

    [Fact]
    public void HasActionPolicies_HandlerImplementsAuthorizationMarker_ReturnsTrue()
    {
        var result = PolicyResolver.HasActionPolicies(typeof(NoAuthorization), new object[] { new NoAuthorizationHandlerAuthorizationHandler() });

        Assert.True(result);
    }

    [Fact]
    public void HasActionPolicies_ActionImplementsIActionAuthorizationOnly_ReturnsTrue()
    {
        var result = PolicyResolver.HasActionPolicies(typeof(ActionAuthorizedByInterface), Array.Empty<object>());

        Assert.True(result);
    }

    [AnonymousPolicy]
    private class ActionAuthorizedByAttr : IMediatorAction;

    [AnonymousPolicy]
    private class ActionAuthorizedByAttrAndInterface : IMediatorAction, IActionAuthorization
    {
        public IPolicy Authorize()
        {
            return IdentityPolicy.Anonymous();
        }
    }

    private class ActionAuthorizedByInterface : IMediatorAction, IActionAuthorization
    {
        public IPolicy Authorize()
        {
            return IdentityPolicy.Anonymous();
        }
    }

    private class NoAuthorization : IMediatorAction;

    [AnonymousPolicy]
    private class NoAuthorizationHandlerAttribute;

    [AnonymousPolicy]
    private interface IAnonamousAction : IMediatorAction;

    private class AnonamousAction : IAnonamousAction;

    private class NoAuthorizationHandler : IMediatorHandler<IMediatorAction>
    {
        public Task Handle(IMediatorAction action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private class NoAuthorizationHandlerAuthorizationHandler : IHandlerAuthorization<IMediatorAction>
    {
        public IPolicy Authorize(IMediatorAction action)
        {
            return IdentityPolicy.Anonymous();
        }
    }

    private class NoAuthorizationHandlerAuthorizationAsyncHandler : IHandlerAuthorizationAsync<IMediatorAction>
    {
        public Task<IPolicy> AuthorizeAsync(IMediatorAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult<IPolicy>(IdentityPolicy.Anonymous());
        }
    }

    [RolePolicy("Admin")]
    private class ActionRequiringRole : IMediatorAction;

    private class NullPolicySyncHandler : IHandlerAuthorization<IMediatorAction>
    {
        public IPolicy Authorize(IMediatorAction action) => null!;
    }

    private class NullPolicyAsyncHandler : IHandlerAuthorizationAsync<IMediatorAction>
    {
        public Task<IPolicy> AuthorizeAsync(IMediatorAction action, CancellationToken cancellationToken) => Task.FromResult<IPolicy>(null!);
    }

    private async Task RunGetPolicies(IMediatorAction action, int expectedCount, params object[] handlers)
    {
        var policies = await PolicyResolver.GetPolicies(action, handlers, CancellationToken.None);
        var count = policies.Count();
        Assert.Equal(expectedCount, count);
    }

    private async Task RunCheckPolicies(IMediatorAction action, AuthorizationExceptionTypes expectedCode, params object[] handlers)
    {
        _services
            .GetService(typeof(INodeFormatter))
            .Returns(new DefaultNodeFormatter());
        var ex = await Assert.ThrowsAsync<AuthorizationException>(async () =>
        {
            await PolicyResolver.CheckPolicies(_services, action, handlers, CancellationToken.None);
        });
        Assert.Equal(expectedCode, ex.Type);
    }
}