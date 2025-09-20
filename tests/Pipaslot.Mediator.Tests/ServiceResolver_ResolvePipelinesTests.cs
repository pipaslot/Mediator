using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_ResolvePipelinesTests
{
    [Test]
    [Arguments(1, typeof(BeforeMiddleware))]
    [Arguments(2, typeof(AfterMiddleware))]
    [Arguments(3, typeof(IExecutionMiddleware))]
    public async Task QueryPath(int position, Type expectedMiddleware)
    {
        var action = new FakeQuery { ExecuteHandlers = false };
        var sp = CreateServiceResolver();
        var sut = sp.GetConcreteMediator();
        var middlewares = sut.GetPipeline(action, false);
        await VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    [Test]
    [Arguments(true, 1, typeof(PipelineMiddleware))]
    [Arguments(true, 2, typeof(PipelineNestedMiddleware))]
    [Arguments(true, 3, typeof(IExecutionMiddleware))]
    [Arguments(false, 1, typeof(PipelineMiddleware))]
    [Arguments(false, 2, typeof(IExecutionMiddleware))]
    public async Task CommandPathNested(bool enableNested, int position, Type expectedMiddleware)
    {
        var action = new FakeCommand { ExecuteNested = enableNested };
        var sp = CreateServiceResolver();
        var sut = sp.GetConcreteMediator();
        var middlewares = sut.GetPipeline(action, false);
        await VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    private async Task VerifyMiddleware(IEnumerable<Mediator.MiddlewarePair> middlewares, int position,
        Type expectedMiddleware)
    {
        var actual = middlewares.Skip(position - 1).First().ResolvableType;
        await Assert.That(actual).IsEqualTo(expectedMiddleware);
    }

    [Test]
    public async Task ActionMatchingMultiplePipelines_ThrowExcepton()
    {
        var action = new FakeCommand();
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddPipelineForAction<ICommand>(x => { });
            c.AddPipeline(x => true, x => { });
        });
        var sut = sp.GetConcreteMediator();
        await Assert.That(async () =>
        {
            var pipeline = sut.GetPipeline(action, false).ToArray();
            await Assert.That(pipeline).IsNotNull();
        }).Throws<MediatorException>();
    }

    private static IServiceProvider CreateServiceResolver()
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
        return sp;
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