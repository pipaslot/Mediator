using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.FakeActions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Middlewares
{
    public class SingleInterfaceHandlerExecutionMiddlewareTests
    {
        [Fact]
        public async Task Request1WithoutHandler_ThrowException()
        {
            var services = Factory.CreateServiceProvider();
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await RunRequest1(services);
            });
        }

        [Fact]
        public async Task Request1WithSingleHandler_ExecuteHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleInterfaceHandler.RequestHandler>();
            var context = await RunRequest1(services);
            Assert.Equal(1, context.ExecutedHandlers);
        }

        [Fact]
        public async Task Request2WithSingleHandler_ExecuteHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleInterfaceHandler.RequestHandler>();
            var context = await RunRequest2(services);
            Assert.Equal(1, context.ExecutedHandlers);
        }

        [Fact]
        public async Task Request1WithMultipleHandlers_ThrowException()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleInterfaceHandler.RequestHandler, SingleInterfaceHandler.RequestHandler>();
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await RunRequest1(services);
            });
        }

        [Fact]
        public async Task Message1WithoutHandler_ThrowException()
        {
            var services = Factory.CreateServiceProvider();
            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await RunMessage1(services);
            });
        }

        [Fact]
        public async Task Message1WithSingleHandler_ExecuteHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleInterfaceHandler.MessageHandler>();
            var context = await RunMessage1(services);
            Assert.Equal(1, context.ExecutedHandlers);
        }

        [Fact]
        public async Task Message2WithSingleHandler_ExecuteHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleInterfaceHandler.MessageHandler>();
            var context = await RunMessage2(services);
            Assert.Equal(1, context.ExecutedHandlers);
        }

        [Fact]
        public async Task Message1WithMultipleHandlers_ThrowException()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SingleInterfaceHandler.MessageHandler, SingleInterfaceHandler.MessageHandler>();
            await Assert.ThrowsAsync<Exception>(async () =>
           {
               await RunMessage1(services);
           });
        }

        private async Task<MediatorContext> RunRequest1(IServiceProvider services)
        {
            var action = new SingleInterfaceHandler.RequestInterface1(true);
            return await Run(services, action);
        }
        private async Task<MediatorContext> RunRequest2(IServiceProvider services)
        {
            var action = new SingleInterfaceHandler.RequestInterface2(true);
            return await Run(services, action);
        }
        private async Task<MediatorContext> RunMessage1(IServiceProvider services)
        {
            var action = new SingleInterfaceHandler.MessageInterface1(true);
            return await Run(services, action);
        }
        private async Task<MediatorContext> RunMessage2(IServiceProvider services)
        {
            var action = new SingleInterfaceHandler.MessageInterface2(true);
            return await Run(services, action);
        }

        private async Task<MediatorContext> Run<TAction>(IServiceProvider services, TAction action)
        {
            var sut = new SingleInterfaceHandlerExecutionMiddleware(services);
            var context = new MediatorContext();
            var next = Factory.CreateMiddlewareDelegate();
            await sut.Invoke(action, context, next, CancellationToken.None);
            return context;
        }
    }
}
