using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests
{
    public class ServiceResolver_ResolvePipelinesTests
    {
        [Theory]
        [InlineData(1, typeof(BeforeMiddleware))]
        [InlineData(2, typeof(AfterMiddleware))]
        [InlineData(3, typeof(HandlerExecutionMiddleware))]
        public void QueryPath(int position, Type expectedMiddleware)
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(new FakeQuery { ExecuteHandlers = false });
            VerifyMiddleware(middlewares, position, expectedMiddleware);
        }

        [Theory]
        [InlineData(true, 1, typeof(PipelineMiddleware))]
        [InlineData(true, 2, typeof(PipelineNestedMiddleware))]
        [InlineData(true, 3, typeof(HandlerExecutionMiddleware))]

        [InlineData(false, 1, typeof(PipelineMiddleware))]
        [InlineData(false, 2, typeof(HandlerExecutionMiddleware))]
        public void CommandPathNested(bool enableNested, int position, Type expectedMiddleware)
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(new FakeCommand { ExecuteNested = enableNested });
            VerifyMiddleware(middlewares, position, expectedMiddleware);
        }

        private void VerifyMiddleware(IEnumerable<(IMediatorMiddleware Instance, object[]? Parameters)> middlewares, int position, Type expectedMiddleware)
        {
            var actual = middlewares.Skip(position - 1).First().Instance.GetType();
            Assert.Equal(expectedMiddleware, actual);
        }

        private static Mediator CreateServiceResolver()
        {
            var sp = Factory.CreateServiceProvider(c =>
            {
                c.Use<BeforeMiddleware>();
                c.AddPipelineForAction<ICommand>(x => x
                    .Use<PipelineMiddleware>()
                    .UseWhen<PipelineNestedMiddleware>(a => a is FakeCommand c && c.ExecuteNested)
                )
                .Use<AfterMiddleware>();
            });
            return (Mediator)sp.GetService<IMediator>();
        }

        [Fact]
        public void ActionMatchingMultiplePipelines_ThrowExcepton()
        {
            var sp = Factory.CreateServiceProvider(c =>
            {
                c.AddPipelineForAction<ICommand>(x => { });
                c.AddPipeline(x => true, x => { });
            });
            var sut = (Mediator)sp.GetService<IMediator>();
            Assert.Throws<MediatorException>(() =>
            {
                sut.GetPipeline(new FakeCommand()).ToArray();
            });
        }

        public interface IQuery : IRequest { }
        public interface IQuery<out TResponse> : IRequest<TResponse>, IQuery { }
        public class FakeQuery : IQuery<object>
        {
            public bool ExecuteHandlers { get; set; }
        }
        public interface ICommand : IMessage { }
        public class FakeCommand : ICommand
        {
            public bool ExecuteNested { get; set; }
        }

        public class BeforeMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }

        public class PipelineMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }

        public class PipelineNestedMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }

        public class AfterMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }
    }
}
