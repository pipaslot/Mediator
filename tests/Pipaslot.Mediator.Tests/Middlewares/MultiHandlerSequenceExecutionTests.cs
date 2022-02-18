using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class MultiHandlerSequenceExecutionTests
    {
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
        public async Task RequestWithSingleHandler_ExecuteHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.RequestHandler1>();
            var context = await RunRequest(services);
            Assert.Equal(1, context.ExecutedHandlers);
        }

        [Fact]
        public async Task RequestWithMultipleHandlers_ThrowException()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.RequestHandler1, SequenceHandler.RequestHandler2>();
            var context = await RunRequest(services);
            Assert.Equal(2, context.ExecutedHandlers);
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
        public async Task MessageWithSingleHandler_ExecuteHandler()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.MessageHandler1>();
            var context = await RunMessage(services);
            Assert.Equal(1, context.ExecutedHandlers);
        }


        [Fact]
        public async Task MessageWithMultipleHandlers_ThrowException()
        {
            var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.MessageHandler1, SequenceHandler.MessageHandler2>();
            var context = await RunMessage(services);
            Assert.Equal(2, context.ExecutedHandlers);
        }

        //TODO Tests for ISequenceHandler.Order

        private async Task<MediatorContext> RunRequest(IServiceProvider services)
        {
            var action = new SequenceHandler.Request(true);
            return await Run(services, action);
        }
        private async Task<MediatorContext> RunMessage(IServiceProvider services)
        {
            var action = new SequenceHandler.Message(true);
            return await Run(services, action);
        }

        private async Task<MediatorContext> Run(IServiceProvider services, IMediatorAction action)
        {
            var sut = new HandlerExecutionMiddleware(services);
            var context = new MediatorContext(action, CancellationToken.None);
            var next = Factory.CreateMiddlewareDelegate();
            await sut.Invoke(context, next);
            return context;
        }
    }
}
