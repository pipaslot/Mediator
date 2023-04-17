using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E
{
    public class BasicFailing
    {
        [Fact]
        public async Task Execute_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Execute_NotEmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.Equal(SingleHandler.RequestException.DefaultMessage, result.GetErrorMessage());
        }

        [Fact]
        public async Task Execute_NullResult()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Execute(new SingleHandler.Request(false));
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task ExecuteUnhandled_ThrowOriginalException()
        {
            var sut = Factory.CreateMediator();
            var ex = await Assert.ThrowsAsync<SingleHandler.RequestException>(async () =>
            {
                await sut.ExecuteUnhandled(new SingleHandler.Request(false));
            });
            // We do not care about the error message as we only expect the original exception
        }

        [Fact]
        public async Task Dispatch_SuccessAsFalse()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.False(result.Success);
        }

        [Fact]
        public async Task Dispatch_NotEmptyErrorMessage()
        {
            var sut = Factory.CreateMediator();
            var result = await sut.Dispatch(new SingleHandler.Message(false));
            Assert.Equal(SingleHandler.MessageException.DefaultMessage, result.GetErrorMessage());
        }

        [Fact]
        public async Task DispatchUnhandled_ThrowOriginalException()
        {
            var sut = Factory.CreateMediator();
            var ex = await Assert.ThrowsAsync<SingleHandler.MessageException>(async () =>
            {
                await sut.DispatchUnhandled(new SingleHandler.Message(false));
            });
            // We do not care about the error message as we only expect the original exception
        }
    }
}
