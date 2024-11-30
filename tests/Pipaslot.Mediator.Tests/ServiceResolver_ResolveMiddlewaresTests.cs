using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_ResolveMiddlewaresTests
{
    [Theory]
    [InlineData(false, 1, typeof(NotificationPropagationMiddleware))]
    [InlineData(false, 2, typeof(BeforeMiddleware))]
    [InlineData(false, 3, typeof(QueryMiddleware))]
    [InlineData(false, 4, typeof(Query2Middleware))]
    [InlineData(false, 5, typeof(DefaultMiddleware))]
    [InlineData(false, 6, typeof(HandlerExecutionMiddleware))]
    [InlineData(true, 1, typeof(NotificationPropagationMiddleware))]
    [InlineData(true, 2, typeof(BeforeMiddleware))]
    [InlineData(true, 3, typeof(QueryMiddleware))]
    [InlineData(true, 4, typeof(HandlerExecutionMiddleware))]
    public void QueryPath(bool executeHanldersInFirstMap, int position, Type expectedMiddleware)
    {
        var sut = CreateServiceResolver();
        var middlewares = sut.GetPipeline(new FakeQuery { ExecuteHandlers = executeHanldersInFirstMap });
        VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    [Theory]
    [InlineData(true, 1, typeof(NotificationPropagationMiddleware))]
    [InlineData(true, 2, typeof(BeforeMiddleware))]
    [InlineData(true, 3, typeof(CommandMiddleware))]
    [InlineData(true, 4, typeof(CommandNestedMiddleware))]
    [InlineData(true, 5, typeof(DefaultMiddleware))]
    [InlineData(true, 6, typeof(HandlerExecutionMiddleware))]
    [InlineData(false, 1, typeof(NotificationPropagationMiddleware))]
    [InlineData(false, 2, typeof(BeforeMiddleware))]
    [InlineData(false, 3, typeof(CommandMiddleware))]
    [InlineData(false, 4, typeof(DefaultMiddleware))]
    [InlineData(false, 5, typeof(HandlerExecutionMiddleware))]
    public void CommandPath(bool enableNested, int position, Type expectedMiddleware)
    {
        var sut = CreateServiceResolver();
        var middlewares = sut.GetPipeline(new FakeCommand { ExecuteNested = enableNested });
        VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    [Theory]
    [InlineData(1, typeof(NotificationPropagationMiddleware))]
    [InlineData(2, typeof(BeforeMiddleware))]
    [InlineData(3, typeof(DefaultMiddleware))]
    [InlineData(4, typeof(HandlerExecutionMiddleware))]
    public void DefaultPath(int position, Type expectedMiddleware)
    {
        var sut = CreateServiceResolver();
        var middlewares = sut.GetPipeline(new FakeNotification());
        VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    [Theory]
    [InlineData(1, true, typeof(NotificationPropagationMiddleware))]
    [InlineData(2, true, typeof(QueryMiddleware))]
    [InlineData(3, true, typeof(Query2Middleware))]
    [InlineData(4, true, typeof(HandlerExecutionMiddleware))]
    [InlineData(1, false, typeof(NotificationPropagationMiddleware))]
    [InlineData(2, false, typeof(HandlerExecutionMiddleware))]
    public void UseWhen(int position, bool applyCustom, Type expectedMiddleware)
    {
        var sut = Factory.CreateInternalMediator(c =>
        {
            c.UseWhen(a => applyCustom, s => s.Use<QueryMiddleware>());
            c.UseWhen((a, s) => applyCustom, s => s.Use<Query2Middleware>());
        });
        var middlewares = sut.GetPipeline(new FakeNotification());
        VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    private void VerifyMiddleware(IEnumerable<(IMediatorMiddleware Instance, object[]? Parameters)> middlewares, int position,
        Type expectedMiddleware)
    {
        var actual = middlewares.Skip(position - 1).First().Instance.GetType();
        Assert.Equal(expectedMiddleware, actual);
    }

    private static Mediator CreateServiceResolver()
    {
        var sp = Factory.CreateServiceProvider(c => c
            .Use<BeforeMiddleware>()
            .UseWhenAction<IQuery>(x => x
                .Use<QueryMiddleware>()
                .UseWhen(a => a is FakeQuery query && query.ExecuteHandlers, y => y.UseHandlerExecution())
            )
            .UseWhenAction<ICommand>(x => x
                .Use<CommandMiddleware>()
                .UseWhen<CommandNestedMiddleware>(a => a is FakeCommand command && command.ExecuteNested)
            )
            .UseWhenAction<IQuery, Query2Middleware>()
            .Use<DefaultMiddleware>()
        );
        return (Mediator)sp.GetRequiredService<IMediator>();
    }

    public interface IQuery : IRequest;

    public interface IQuery<out TResponse> : IRequest<TResponse>, IQuery;

    public class FakeQuery : IQuery<object>
    {
        public bool ExecuteHandlers { get; init; }
    }

    public interface ICommand : IMessage;

    public class FakeCommand : ICommand
    {
        public bool ExecuteNested { get; init; }
    }

    public interface INotification : IMessage;

    public class FakeNotification : INotification;

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

    public class Query2Middleware : IMediatorMiddleware
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