using Xunit;
using System.Threading.Tasks;
using System.Linq;
using Pipaslot.Mediator.Services;
using Pipaslot.Mediator.Middlewares;
using System;

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
            Assert.Equal(typeof(HandlerExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }
        [Fact]
        public void DirectUse_ResolveActionSpecificPipelineWithMiltiHandler()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeCommand));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(CommandMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(HandlerExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }
        [Fact]
        public void DirectUse_ResolveDefaultPipeline()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeNotification));

            Assert.Equal(2, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(HandlerExecutionMiddleware), middlewares.Skip(1).First().GetType());
        }

        [Fact]
        public void AddPipeline_ResolveActionSpecificPipeline()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeQuery));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(QueryMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(HandlerExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }
        [Fact]
        public void AddPipeline_ResolveActionSpecificPipelineWithMiltiHandlerAndRegisteredViaFluentInterface()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeCommand));

            Assert.Equal(3, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(CommandMiddleware), middlewares.Skip(1).First().GetType());
            Assert.Equal(typeof(HandlerExecutionMiddleware), middlewares.Skip(2).First().GetType());
        }

        [Fact]
        public void AddPipeline_ResolveDefaultPipeline()
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeNotification));

            Assert.Equal(2, middlewares.Count());
            Assert.Equal(typeof(SharedMiddleware), middlewares.First().GetType());
            Assert.Equal(typeof(HandlerExecutionMiddleware), middlewares.Skip(1).First().GetType());
        }

        private static IServiceProvider CreateServiceResolver()
        {
            return Factory.CreateServiceProvider(c => c
                .AddPipeline<IQuery>()
                    .Use<SharedMiddleware>()
                    .Use<QueryMiddleware>()
                .AddPipeline<ICommand>()
                    .Use<SharedMiddleware>()
                    .Use<CommandMiddleware>()
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
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }

        public class CommandMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }

        public class SharedMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }
    }
}
