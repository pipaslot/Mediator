using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_ResolvePipelinesTests
{
    [Theory]
    [InlineData(1, typeof(BeforeMiddleware))]
    [InlineData(2, typeof(AfterMiddleware))]
    [InlineData(3, typeof(IExecutionMiddleware))]
    public void QueryPath(int position, Type expectedMiddleware)
    {
        var action = new FakeQuery { ExecuteHandlers = false };
        var sp = CreateServiceResolver();
        var sut = sp.GetConcreteMediator();
        var context = MediatorContextFactory.Create(sp, action);
        var middlewares = sut.GetPipeline(action, context, false);
        VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    [Theory]
    [InlineData(true, 1, typeof(PipelineMiddleware))]
    [InlineData(true, 2, typeof(PipelineNestedMiddleware))]
    [InlineData(true, 3, typeof(IExecutionMiddleware))]
    
    [InlineData(false, 1, typeof(PipelineMiddleware))]
    [InlineData(false, 2, typeof(IExecutionMiddleware))]
    public void CommandPathNested(bool enableNested, int position, Type expectedMiddleware)
    {
        var action = new FakeCommand { ExecuteNested = enableNested };
        var sp = CreateServiceResolver();
        var sut = sp.GetConcreteMediator();
        var context = MediatorContextFactory.Create(sp, action);
        var middlewares = sut.GetPipeline(action, context, false);
        VerifyMiddleware(middlewares, position, expectedMiddleware);
    }

    private void VerifyMiddleware(IEnumerable<Mediator.MiddlewarePair> middlewares, int position,
        Type expectedMiddleware)
    {
        var actual = middlewares.Skip(position - 1).First().ResolvableType;
        Assert.Equal(expectedMiddleware, actual);
    }

    [Fact]
    public void ActionMatchingMultiplePipelines_ThrowExcepton()
    {
        var action = new FakeCommand();
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddPipelineForAction<ICommand>(x => { });
            c.AddPipeline(x => true, x => { });
        });
        var sut = sp.GetConcreteMediator();
        var context = MediatorContextFactory.Create(sp, action);
        Assert.Throws<MediatorException>(() =>
        {
            var pipeline = sut.GetPipeline(action, context, false).ToArray();
            Assert.NotNull(pipeline);
        });
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