using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Authorization.Formatting;
using System;
using System.Linq;
using System.Threading;

namespace Pipaslot.Mediator.Tests.Authorization;

/// <summary>
/// This tests simulate already resolved combination of single action and one or more actions. 
/// To make the tests simple, there are not connections between handlers and action like real usage.
/// </summary>
public class PolicyResolverTests
{
    private readonly Mock<IServiceProvider> _services = new();

    [Test]
    public async Task CheckPolicies_NoAuthorization_ThrowException()
    {
        await RunCheckPolicies(
            new NoAuthorization(),
            AuthorizationExceptionTypes.NoAuthorization);
    }

    [Test]
    public async Task GetPolicies_SecuredSyncHandler_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new NoAuthorization(),
            1,
            new NoAuthorizationHandlerAuthorizationHandler());
    }

    [Test]
    public async Task GetPolicies_SecuredAsyncHandler_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new NoAuthorization(),
            1,
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Test]
    public async Task GetPolicies_SecuredAttrHandler_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new NoAuthorization(),
            1,
            new NoAuthorizationHandlerAttribute());
    }

    [Test]
    public async Task GetPolicies_CombineSyncAndAsyncHandler_ResolveTwoPolicies()
    {
        await RunGetPolicies(
            new NoAuthorization(),
            2,
            new NoAuthorizationHandlerAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Test]
    public async Task CheckPolicies_MultiHandlersButOneHandlerIsUnsecured_ThrowException()
    {
        await RunCheckPolicies(
            new NoAuthorization(),
            AuthorizationExceptionTypes.UnauthorizedHandler,
            new NoAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Test]
    public async Task GetPolicies_AuthorizedActionByAttr_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new ActionAuthorizedByAttr(),
            1);
    }

    [Test]
    public async Task GetPolicies_AuthorizedActionByInterface_ResolveSinglePolicy()
    {
        await RunGetPolicies(
            new ActionAuthorizedByInterface(),
            1);
    }

    [Test]
    public async Task GetPolicies_CombineAllAvailablePolicies_ResolveTwoPolicies()
    {
        await RunGetPolicies(
            new ActionAuthorizedByAttrAndInterface(),
            5,
            new NoAuthorizationHandlerAttribute(),
            new NoAuthorizationHandlerAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
    }

    [Test]
    public async Task GetPolicies_ReadPolicyFromInterfaces()
    {
        await RunGetPolicies(
            new AnonamousAction(),
            1);
    }


    [Test]
    public async Task CheckPolicies_AuthorizedActionAndSyncAndAsyncHandlersAndUnauthorizedHandler_ThrowException()
    {
        await RunCheckPolicies(
            new ActionAuthorizedByAttr(),
            AuthorizationExceptionTypes.UnauthorizedHandler,
            new NoAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationHandler(),
            new NoAuthorizationHandlerAuthorizationAsyncHandler());
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

    private async Task RunGetPolicies(IMediatorAction action, int expectedCount, params object[] handlers)
    {
        var policies = await PolicyResolver.GetPolicies(action, handlers, CancellationToken.None);
        var count = policies.Count();
        Assert.Equal(expectedCount, count);
    }

    private async Task RunCheckPolicies(IMediatorAction action, AuthorizationExceptionTypes expectedCode, params object[] handlers)
    {
        _services
            .Setup(s => s.GetService(typeof(INodeFormatter)))
            .Returns(new DefaultNodeFormatter());
        var ex = await Assert.ThrowsAsync<AuthorizationException>(async () =>
        {
            await PolicyResolver.CheckPolicies(_services.Object, action, handlers, CancellationToken.None);
        });
        Assert.Equal(expectedCode, ex.Type);
    }
}