using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E
{
    /// <summary>
    /// This case simulates validator intercepting the processing and returning error message. The handler is not executed on purpose.
    /// </summary>
    public class NoHandlerAndErrorReturned
    {
        private const string Error = "Fake error";

        [Fact]
        public async Task Execute_SuccessAsFalse()
        {
            var sut = Factory.CreateConfiguredMediator(c => c.Use<AddErrorAndEndMiddleware>());
            var result = await sut.Execute(new SingleHandler.Request(true));
            Assert.False(result.Success);
            Assert.Equal(Error, result.GetErrorMessage());
            Assert.Null(result.Result);
        }

        [Fact]
        public async Task ExecuteUnhandled_ThrowException()
        {
            var sut = Factory.CreateConfiguredMediator(c => c.Use<AddErrorAndEndMiddleware>());
            var action = new SingleHandler.Request(true);
            var ex = await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.ExecuteUnhandled(action);
            });
            var context = Factory.FakeContext(action);
            Assert.Equal(MediatorExecutionException.CreateForUnhandledError($"'{Error}'", context).Message, ex.Message);
        }

        [Fact]
        public async Task Dispatch_SuccessAsFalse()
        {
            var sut = Factory.CreateConfiguredMediator(c => c.Use<AddErrorAndEndMiddleware>());
            var result = await sut.Dispatch(new SingleHandler.Message(true));
            Assert.False(result.Success);
            Assert.Equal(Error, result.GetErrorMessage());
        }

        [Fact]
        public async Task Dispatchnhandled_ThrowException()
        {
            var sut = Factory.CreateConfiguredMediator(c => c.Use<AddErrorAndEndMiddleware>());
            var action = new SingleHandler.Message(true);
            var ex = await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.DispatchUnhandled(action);
            });
            var context = Factory.FakeContext(action);
            Assert.Equal(MediatorExecutionException.CreateForUnhandledError($"'{Error}'", context).Message, ex.Message);
        }

        public class AddErrorAndEndMiddleware : IMediatorMiddleware
        {
            public Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                context.AddError(Error);
                return Task.CompletedTask;
            }
        }
    }
}
