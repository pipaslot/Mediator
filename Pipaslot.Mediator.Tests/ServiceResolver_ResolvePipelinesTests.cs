using Xunit;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Pipaslot.Mediator.Services;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Tests
{
    public class ServiceResolver_ResolvePipelinesTests
    {
        [Fact]
        public void DirectUse_ResolveActionSpecificPipeline()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeQuery));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(QueryMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(SingleHandlerExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }
        [Fact]
        public void DirectUse_ResolveActionSpecificPipelineWithMiltiHandler()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeCommand));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(CommandMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(MultiHandlerConcurrentExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }
        [Fact]
        public void DirectUse_ResolveDefaultPipeline()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeNotification));

            Assert.Equal(2, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(SingleHandlerExecutionMiddleware), middlewares.Skip(1).First().GetType());
        }

        [Fact]
        public void AddPipeline_ResolveActionSpecificPipeline()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeQuery));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(QueryMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(SingleHandlerExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }
        [Fact]
        public void AddPipeline_ResolveActionSpecificPipelineWithMiltiHandlerAndRegisteredViaFluentInterface()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeCommand));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(CommandMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(MultiHandlerConcurrentExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }

        [Fact]
        public void AddPipeline_ResolveDefaultPipeline()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeNotification));

            Assert.Equal(2, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(SingleHandlerExecutionMiddleware), middlewares.Skip(1).First().GetType());
        }

        private ServiceResolver CreateServiceResolver()
        {
            return Factory.CreateServiceResolver(c => c
                .AddPipeline<IQuery>()
                    .Use<SharedMiddleware>()
                    .Use<QueryMiddleware>()
                .AddPipeline<ICommand>()
                    .Use<SharedMiddleware>()
                    .Use<CommandMiddleware>()
                    .UseConcurrentMultiHandler()
                .AddDefaultPipeline()
                    .Use<SharedMiddleware>());
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
