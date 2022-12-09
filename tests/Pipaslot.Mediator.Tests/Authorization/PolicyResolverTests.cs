using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Authorization
{
    /// <summary>
    /// This tests simulate already resolved combination of single action and one or more actions. 
    /// To make the tests simple, there are not connections between handlers and action like real usage.
    /// </summary>
    public class PolicyResolverTests
    {
        private Mock<IServiceProvider> _services = new();

        [Fact]
        public async Task CheckPolicies_NoAuthorization_ThrowException() => await RunCheckPolicies(
                new NoAuthorization(),
                AuthorizationException.NoAuthorizationCode);

        [Fact]
        public async Task GetPolicies_SecuredSyncHandler_ResolveSinglePolicy() => await RunGetPolicies(
                new NoAuthorization(),
                1,
                new NoAuthorizationHandlerAuthorizationHandler());

        [Fact]
        public async Task GetPolicies_SecuredAsyncHandler_ResolveSinglePolicy() => await RunGetPolicies(
                new NoAuthorization(),
                1,
                new NoAuthorizationHandlerAuthorizationAsyncHandler());

        [Fact]
        public async Task GetPolicies_SecuredAttrHandler_ResolveSinglePolicy() => await RunGetPolicies(
                new NoAuthorization(),
                1,
                new NoAuthorizationHandlerAttribute());

        [Fact]
        public async Task GetPolicies_CombineSyncAndAsyncHandler_ResolveTwoPolicies() => await RunGetPolicies(
                new NoAuthorization(),
                2,
                new NoAuthorizationHandlerAuthorizationHandler(),
                new NoAuthorizationHandlerAuthorizationAsyncHandler());
        [Fact]
        public async Task CheckPolicies_MultiHandlersButOneHandlerIsUnsecured_ThrowException() => await RunCheckPolicies(
                new NoAuthorization(),
                AuthorizationException.UnauthorizedHandlerCode,
                new NoAuthorizationHandler(),
                new NoAuthorizationHandlerAuthorizationAsyncHandler());

        [Fact]
        public async Task GetPolicies_AuthorizedActionByAttr_ResolveSinglePolicy() => await RunGetPolicies(
                new ActionAuthorizedByAttr(),
                1);

        [Fact]
        public async Task GetPolicies_AuthorizedActionByInterface_ResolveSinglePolicy() => await RunGetPolicies(
                new ActionAuthorizedByInterface(),
                1);

        [Fact]
        public async Task GetPolicies_CombineAllAvailablePolicies_ResolveTwoPolicies() => await RunGetPolicies(
                new ActionAuthorizedByAttrAndInterface(),
                5,
                new NoAuthorizationHandlerAttribute(),
                new NoAuthorizationHandlerAuthorizationHandler(),
                new NoAuthorizationHandlerAuthorizationAsyncHandler());

        [Fact]
        public async Task GetPolicies_ReadPolicyFromInterfaces() => await RunGetPolicies(
                new AnonamousAction(),
                1);


        [Fact]
        public async Task CheckPolicies_AuthorizedActionAndSyncAndAsyncHandlersAndUnauthorizedHandler_ThrowException() => await RunCheckPolicies(
                new ActionAuthorizedByAttr(),
                AuthorizationException.UnauthorizedHandlerCode,
                new NoAuthorizationHandler(),
                new NoAuthorizationHandlerAuthorizationHandler(),
                new NoAuthorizationHandlerAuthorizationAsyncHandler());

        [AnonymousPolicy]
        private class ActionAuthorizedByAttr : IMediatorAction { }
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
        private class NoAuthorization : IMediatorAction { }
        [AnonymousPolicy]
        private class NoAuthorizationHandlerAttribute { }

        [AnonymousPolicy]
        private interface IAnonamousAction : IMediatorAction { }
        private class AnonamousAction : IAnonamousAction { }
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
                return Task.FromResult((IPolicy)IdentityPolicy.Anonymous());
            }
        }
        private async Task RunGetPolicies(IMediatorAction action, int expectedCount, params object[] handlers)
        {
            var policies = await PolicyResolver.GetPolicies(action, handlers, CancellationToken.None);
            var count = policies.Count();
            Assert.Equal(expectedCount, count);
        }

        private async Task RunCheckPolicies(IMediatorAction action, int expectedCode, params object[] handlers)
        {
            var ex = await Assert.ThrowsAsync<AuthorizationException>(async () =>
            {
                await PolicyResolver.CheckPolicies(_services.Object, action, handlers, CancellationToken.None);
            });
            Assert.Equal(expectedCode, ex.Code);
        }
    }
}
