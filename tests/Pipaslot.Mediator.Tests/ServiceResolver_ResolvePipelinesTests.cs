﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_ResolvePipelinesTests
{
    [Theory]
    [InlineData(1, typeof(NotificationPropagationMiddleware))]
    [InlineData(2, typeof(BeforeMiddleware))]
    [InlineData(3, typeof(AfterMiddleware))]
    [InlineData(4, typeof(HandlerExecutionMiddleware))]
    public void QueryPath(int position, Type expectedMiddleware)
    {
        var sut = CreateServiceResolver();
        var middlewares = sut.GetPipeline(new FakeQuery { ExecuteHandlers = false });
        VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    [Theory]
    [InlineData(true, 1, typeof(NotificationPropagationMiddleware))]
    [InlineData(true, 2, typeof(PipelineMiddleware))]
    [InlineData(true, 3, typeof(PipelineNestedMiddleware))]
    [InlineData(true, 4, typeof(HandlerExecutionMiddleware))]
    [InlineData(false, 1, typeof(NotificationPropagationMiddleware))]
    [InlineData(false, 2, typeof(PipelineMiddleware))]
    [InlineData(false, 3, typeof(HandlerExecutionMiddleware))]
    public void CommandPathNested(bool enableNested, int position, Type expectedMiddleware)
    {
        var sut = CreateServiceResolver();
        var middlewares = sut.GetPipeline(new FakeCommand { ExecuteNested = enableNested });
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
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.Use<BeforeMiddleware>();
            c.AddPipelineForAction<ICommand>(x => x
                    .Use<PipelineMiddleware>()
                    .UseWhen<PipelineNestedMiddleware>(a => a is FakeCommand fakeCommand && fakeCommand.ExecuteNested)
                )
                .Use<AfterMiddleware>();
        });
        return (Mediator)sp.GetRequiredService<IMediator>();
    }

    [Fact]
    public void ActionMatchingMultiplePipelines_ThrowExcepton()
    {
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddPipelineForAction<ICommand>(x => { });
            c.AddPipeline(x => true, x => { });
        });
        var sut = (Mediator)sp.GetRequiredService<IMediator>();
        Assert.Throws<MediatorException>(() =>
        {
            var pipeline = sut.GetPipeline(new FakeCommand()).ToArray();
            Assert.NotNull(pipeline);
        });
    }

    public interface IQuery : IRequest;

    public interface IQuery<out TResponse> : IRequest<TResponse>, IQuery;

    public class FakeQuery : IQuery<object>
    {
        public bool ExecuteHandlers { get; set; }
    }

    public interface ICommand : IMessage;

    public class FakeCommand : ICommand
    {
        public bool ExecuteNested { get; init; }
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