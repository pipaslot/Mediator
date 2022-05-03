using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Authorization
{
    public class MediatorContextPolicyResolveTests
    {
        private Mock<IMediator> _mediator = new();
        private Mock<IServiceProvider> _services = new();

        [Fact]
        public async Task NoAuthorization_ThrowException() => await TestException(
                new NoAuthorization(),
                AuthorizationException.NoAuthorizationCode);

        [Fact]
        public async Task SecuredSyncHandler_ResolveSinglePolicy() => await TestPassing(
                new NoAuthorization(),
                1,
                new NoAuthorizationHandlerAuthorizationHandler());

        [Fact]
        public async Task SecuredAsyncHandler_ResolveSinglePolicy() => await TestPassing(
                new NoAuthorization(),
                1,
                new NoAuthorizationHandlerAuthorizationAsyncHandler());

        [Fact]
        public async Task SecuredAttrHandler_ResolveSinglePolicy() => await TestPassing(
                new NoAuthorization(),
                1,
                new NoAuthorizationHandlerAttribute());

        [Fact]
        public async Task CombineSyncAndAsyncHandler_ResolveTwoPolicies() => await TestPassing(
                new NoAuthorization(),
                2,
                new NoAuthorizationHandlerAuthorizationHandler(),
                new NoAuthorizationHandlerAuthorizationAsyncHandler());
        [Fact]
        public async Task MultiHandlersButOneHandlerIsUnsecured_ThrowException() => await TestException(
                new NoAuthorization(),
                AuthorizationException.UnauthorizedHandlerCode,
                new NoAuthorizationHandler(),
                new NoAuthorizationHandlerAuthorizationAsyncHandler());

        [Fact]
        public async Task AuthorizedActionByAttr_ResolveSinglePolicy() => await TestPassing(
                new ActionAuthorizedByAttr(),
                1);

        [Fact]
        public async Task AuthorizedActionByInterface_ResolveSinglePolicy() => await TestPassing(
                new ActionAuthorizedByInterface(),
                1);

        [Fact]
        public async Task CombineAllAvailablePolicies_ResolveTwoPolicies() => await TestPassing(
                new ActionAuthorizedByAttrAndInterface(),
                5,
                new NoAuthorizationHandlerAttribute(),
                new NoAuthorizationHandlerAuthorizationHandler(),
                new NoAuthorizationHandlerAuthorizationAsyncHandler());

        [Fact]
        public async Task AuthorizedActionAndSyncAndAsyncHandlersAndUnauthorizedHandler_ThrowException() => await TestException(
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
        private async Task TestPassing(IMediatorAction action, int expectedCount, params object[] handlers)
        {
            var sut = Create(action, handlers);
            var policies = await sut.GetPolicies();
            var count = policies.Count();
            Assert.Equal(expectedCount, count);
        }

        private async Task TestException(IMediatorAction action, int expectedCode, params object[] handlers)
        {
            var sut = Create(action, handlers);
            var ex = await Assert.ThrowsAsync<AuthorizationException>(async () =>
            {
                await sut.CheckPolicies();
            });
            Assert.Equal(expectedCode, ex.Code);
        }

        private MediatorContext Create(IMediatorAction action, params object[] handlers)
        {
            var mca = new Mock<IMediatorContextAccessor>();
            return new MediatorContext(_mediator.Object, mca.Object, _services.Object, action, CancellationToken.None, handlers);
        }
    }
}
