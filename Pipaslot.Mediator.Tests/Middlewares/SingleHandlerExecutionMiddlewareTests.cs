using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.FakeActions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Middlewares
{
    public class SingleHandlerExecutionMiddlewareTests
    {

        [Fact]
        public async Task RequestWithSingleHandler_ExecuteHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.RequestHandler>();
            var context = await RunRequest(services);
            Assert.Equal(1, context.ExecutedHandlers);
        }

        [Fact]
        public async Task RequestWithoutHandler_ThrowException()
        {
            var services = Factory.CreateServiceProvider();
            await Assert.ThrowsAsync<MediatorException>(async () =>
            {
                await RunRequest(services);
            });
        }

        [Fact]
        public async Task RequestWithMultipleHandlers_ThrowException()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.RequestHandler, SingleHandler.RequestHandler>();
            await Assert.ThrowsAsync<MediatorException>(async () =>
            {
                await RunRequest(services);
            });
        }

        [Fact]
        public async Task MessageWithSingleHandler_ExecuteHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.MessageHandler>();
            var context = await RunMessage(services);
            Assert.Equal(1, context.ExecutedHandlers);
        }

        [Fact]
        public async Task MessageWithoutHandler_ThrowException()
        {
            var services = Factory.CreateServiceProvider();
            await Assert.ThrowsAsync<MediatorException>(async () =>
            {
                await RunMessage(services);
            });
        }

        [Fact]
        public async Task MessageWithMultipleHandlers_ThrowException()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.MessageHandler, SingleHandler.MessageHandler>();
            await Assert.ThrowsAsync<MediatorException>(async () =>
           {
               await RunMessage(services);
           });
        }

        private async Task<MediatorContext> RunRequest(IServiceProvider services)
        {
            var action = new SingleHandler.Request(true);
            return await Run(services, action);
        }
        private async Task<MediatorContext> RunMessage(IServiceProvider services)
        {
            var action = new SingleHandler.Message(true);
            return await Run(services, action);
        }

        private async Task<MediatorContext> Run<TAction>(IServiceProvider services, TAction action)
        {
            var sut = new SingleHandlerExecutionMiddleware(services);
            var context = new MediatorContext();
            var next = Factory.CreateMiddlewareDelegate();
            await sut.Invoke(action, context, next, CancellationToken.None);
            return context;
        }
    }
}
