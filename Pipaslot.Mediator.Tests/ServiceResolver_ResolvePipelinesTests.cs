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

        [Fact]
        public void HasMultipleDefaultPipeline_MultipleDefaultPipelines_ReturnsTrue()
        {
            var sut = Factory.CreateServiceResolver(c => c
            .AddDefaultPipeline()
                .AddDefaultPipeline());
            var has = sut.HasMultipleDefaultPipelines();

            Assert.True(has);
        }

        [Fact]
        public void HasMultipleDefaultPipelines_SingleDefaultPipelines_ReturnsFalse()
        {
            var sut = Factory.CreateServiceResolver(c => c
                .AddPipeline<IQuery>()
                .AddDefaultPipeline());
            var has = sut.HasMultipleDefaultPipelines();

            Assert.False(has);
        }

        [Fact]
        public void HasMultipleDefaultPipelines_NoDefaultPipelines_ReturnsFalse()
        {
            var sut = Factory.CreateServiceResolver(c => c.AddPipeline<IQuery>());
            var has = sut.HasMultipleDefaultPipelines();

            Assert.False(has);
        }


        [Fact]
        public void IsDefaultPipelineLastOrMissing_NotDefaultPipeline_ReturnsTrue()
        {
            var sut = Factory.CreateServiceResolver(c => c.AddPipeline<IQuery>());
            var has = sut.IsDefaultPipelineLastOrMissing();

            Assert.True(has);
        }
        [Fact]
        public void IsDefaultPipelineLastOrMissing_NotLast_ReturnsFalse()
        {
            var sut = Factory.CreateServiceResolver(c =>
                c.AddDefaultPipeline()
                .AddPipeline<IQuery>());
            var has = sut.IsDefaultPipelineLastOrMissing();

            Assert.False(has);
        }
        [Fact]
        public void IsDefaultPipelineLastOrMissing_Last_ReturnsTrue()
        {
            var sut = Factory.CreateServiceResolver(c => c.AddPipeline<IQuery>()
                .AddDefaultPipeline());
            var has = sut.IsDefaultPipelineLastOrMissing();

            Assert.True(has);
        }

        private ServiceResolver CreateServiceResolver()
        {
            return Factory.CreateServiceResolver(c => c
            .Use<SharedMiddleware>()
                .Use<QueryMiddleware, IQuery>()
                .Use<CommandMiddleware, ICommand>()
                .UseConcurrentMultiHandler<ICommand>()

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
