using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class ServiceResolver_ResolveMiddlewaresTests
    {
        [Theory]
        [InlineData(1, typeof(BeforeMiddleware))]
        [InlineData(2, typeof(QueryMiddleware))]
        [InlineData(3, typeof(HandlerExecutionMiddleware))]
        public void QueryPath(int position, Type expectedMiddleware)
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeQuery));
            VerifyMiddleware(middlewares, position, expectedMiddleware);
        }

        [Theory]
        [InlineData(1, typeof(BeforeMiddleware))]
        [InlineData(2, typeof(CommandMiddleware))]
        [InlineData(3, typeof(HandlerExecutionMiddleware))]
        public void CommandPath(int position, Type expectedMiddleware)
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeCommand));
            VerifyMiddleware(middlewares, position, expectedMiddleware);
        }

        [Theory]
        [InlineData(true, 1, typeof(BeforeMiddleware))]
        [InlineData(true, 2, typeof(CommandMiddleware))]
        [InlineData(true, 3, typeof(CommandNestedMiddleware))]
        [InlineData(true, 4, typeof(HandlerExecutionMiddleware))]

        [InlineData(false, 1, typeof(BeforeMiddleware))]
        [InlineData(false, 2, typeof(CommandMiddleware))]
        [InlineData(false, 3, typeof(HandlerExecutionMiddleware))]
        public void CommandPathNested(bool enableNested, int position, Type expectedMiddleware)
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeCommand));
            VerifyMiddleware(middlewares, position, expectedMiddleware);
        }

        [Theory]
        [InlineData(1, typeof(BeforeMiddleware))]
        [InlineData(2, typeof(DefaultMiddleware))]
        [InlineData(2, typeof(HandlerExecutionMiddleware))]
        public void DefaultPath(int position, Type expectedMiddleware)
        {
            var sut = CreateServiceResolver();
            var middlewares = sut.GetPipeline(typeof(FakeNotification));
            VerifyMiddleware(middlewares, position, expectedMiddleware);
        }

        private void VerifyMiddleware(IEnumerable<IMediatorMiddleware> middlewares, int position, Type expectedMiddleware)
        {
            var actual = middlewares.Skip(position - 1).First();
            var actualType = actual.GetType();
            Assert.Equal(expectedMiddleware, actualType);
        }

        private static IServiceProvider CreateServiceResolver()
        {
            return Factory.CreateServiceProvider(/*c => c
                    .Use<BeforeMiddleware>()
                    .MapWhen<IQuery>(x => x.Use<QueryMiddleware>())
                    .MapWhen<ICommand>(x => x
                        .Use<CommandMiddleware>()
                        .MapWhen(a => a.ExecuteNested, y => y.Use<CommandNestedMiddleware>())
                        )
                    .Use<DefaultMiddleware>()*/
                );
        }

        public interface IQuery : IRequest { }
        public interface IQuery<out TResponse> : IRequest<TResponse>, IQuery { }
        public class FakeQuery : IQuery<object> { }
        public interface ICommand : IMessage { }
        public class FakeCommand : ICommand
        {
            public bool ExecuteNested { get; set; }
        }
        public interface INotification : IMessage { }
        public class FakeNotification : INotification { }

        public class BeforeMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }

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

        public class CommandNestedMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }

        public class DefaultMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
            }
        }
    }
}
