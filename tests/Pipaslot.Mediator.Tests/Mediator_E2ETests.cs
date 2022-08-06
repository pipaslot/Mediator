using Pipaslot.Mediator.Tests.InvalidActions;
using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class Mediator_E2ETests
    {
        #region Execute single handler

        [Fact]
        public async Task Execute_PassingHandler_SuccessAsTrue()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(true));
            Assert.True(result.Success);
        }

        [Fact]
        public async Task Execute_PassingHandler_EmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(true));
            Assert.Equal(string.Empty, result.GetErrorMessage());
        }

        [Fact]
        public async Task Execute_PassingHandler_ResultReturnsDataFromHandler()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(true));
            Assert.Equal(SingleHandler.Response.Instance, result.Result);
        }

        [Fact]
        public async Task Execute_FailingHandler_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Execute_FailingHandlerWithMiddlewareCatchingAllExceptions_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator(c => {
                c.Use<ExceptionConsumingMiddleware>();
            });
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Execute_FailingHandler_NotEmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.Equal(SingleHandler.RequestException.DefaultMessage, result.GetErrorMessage());
        }

        [Fact]
        public async Task Execute_FailingHandler_NullResult()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.Null(result.Result);
        }

        #endregion

        #region ExecuteUnhandled single handler

        [Fact]
        public async Task ExecuteUnhandled_PassingHandler_ReturnsResult()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.ExecuteUnhandled(new SingleHandler.Request(true));
            Assert.Equal(SingleHandler.Response.Instance, result);
        }

        [Fact]
        public async Task ExecuteUnhandled_FailingHandler_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            await Assert.ThrowsAsync<SingleHandler.RequestException>(async () =>
            {
                await sut.ExecuteUnhandled(new SingleHandler.Request(false));
            });
        }

        [Fact]
        public async Task ExecuteUnhandled_NoResult_ThrowException()
        {
            var sut = Factory.CreateMediator(c=>c.Use<NopMiddleware>());
            await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.ExecuteUnhandled(new SingleHandler.Request(true));
            });
        }

        #endregion

        #region Dispatch single handler

        [Fact]
        public async Task Dispatch_PassingHandler_SuccessAsTrue()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(true));
            Assert.True(result.Success);
        }

        [Fact]
        public async Task Dispatch_PassingHandler_EmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(true));
            Assert.Equal(string.Empty, result.GetErrorMessage());
        }

        [Fact]
        public async Task Dispatch_FailingHandler_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Dispatch_FailingHandlerWithMiddlewareCatchingAllExceptions_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator(c => {
                c.Use<ExceptionConsumingMiddleware>();
            });
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Dispatch_FailingHandler_NotEmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.Equal(SingleHandler.MessageException.DefaultMessage, result.GetErrorMessage());
        }

        #endregion

        #region DispatchUnhandled single handler

        [Fact]
        public async Task DispatchUnhandled_PassingHandler_NoAction()
        {
            var sut = Factory.CreateMediator();
            await sut.DispatchUnhandled(new SingleHandler.Message(true));
        }

        [Fact]
        public async Task DispatchUnhandled_FailingHandler_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            await Assert.ThrowsAsync<SingleHandler.MessageException>(async () =>
            {
                await sut.DispatchUnhandled(new SingleHandler.Message(false));
            });
        }


        [Fact]
        public async Task ExecuteUnhandled_ReturnError_ThrowException()
        {
            var sut = Factory.CreateMediator(c => c.Use<AddErrorAndEndMiddleware>());
            await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.ExecuteUnhandled(new SingleHandler.Request(true));
            });
        }

        #endregion
    }
}
