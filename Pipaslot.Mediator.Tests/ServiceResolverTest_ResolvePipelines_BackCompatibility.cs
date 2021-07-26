using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Pipaslot.Mediator.Services;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Tests
{
    [Obsolete]
    public class ServiceResolverTest_ResolvePipelines_backCompatibility
    {
        [Fact]
        public void ResolveActionSpecificPipeline()
        {
            var services = CreateServiceProvider();
            var sut = services.GetRequiredService<ServiceResolver>();
            var middlewares = sut.GetPipeline(typeof(FakeQuery));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(QueryMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(SingleHandlerExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }
        [Fact]
        public void ResolveActionSpecificPipelineWithMiltiHandler()
        {
            var services = CreateServiceProvider();
            var sut = services.GetRequiredService<ServiceResolver>();
            var middlewares = sut.GetPipeline(typeof(FakeCommand));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(CommandMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(MultiHandlerConcurrentExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }
        [Fact]
        public void ResolveDefaultPipeline()
        {
            var services = CreateServiceProvider();
            var sut = services.GetRequiredService<ServiceResolver>();
            var middlewares = sut.GetPipeline(typeof(FakeNotification));

            Assert.Equal(2, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(SingleHandlerExecutionMiddleware), middlewares.Skip(1).First().GetType());
        }

        private IServiceProvider CreateServiceProvider()
        {
            var collection = new ServiceCollection();
            collection.AddMediator()
                .Use<SharedMiddleware>()
                .Use<QueryMiddleware, IQuery>()
                .Use<CommandMiddleware, ICommand>()
                .UseConcurrentMultiHandler<ICommand>();
            return collection.BuildServiceProvider();
        }

        public interface IQuery : IRequest { }
        public interface IQuery<out TResponse> : IRequest<TResponse>, IQuery { }
        public class FakeQuery : IQuery<object> { }
        public interface ICommand : IMessage { }
        public class FakeCommand : ICommand { }
        public interface INotification : IMessage { }
        public class FakeNotification : INotification { }

        public class QueryMiddleware : IMediatorMiddleware
        {
            public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
            {
                await next(context);
            }
        }

        public class CommandMiddleware : IMediatorMiddleware
        {
            public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
            {
                await next(context);
            }
        }

        public class SharedMiddleware : IMediatorMiddleware
        {
            public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
            {
                await next(context);
            }
        }
    }
}
