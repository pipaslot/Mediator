using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_PipelineRegistrationTests
{
    private const string Result1 = "result1";
    private const string Result2 = "result2";

    [Theory]
    [InlineData(typeof(FakeAction1), Result1)]
    [InlineData(typeof(FakeAction2), Result2)]
    public async Task AddPipeline_PipelinesWithUniqueNamesWillBeResolvedIndependently(Type actionType, string expectedResult)
    {
        var sp = Factory.CreateServiceProvider(c => c
            .AddPipeline(c => c is FakeAction1, p =>
                {
                    p.Use<FakeMiddleware1>();
                },
                "name 1")
            .AddPipeline(c => c is FakeAction2, p =>
                {
                    p.Use<FakeMiddleware2>();
                },
                "name 2")
        );
        await AssertAction(sp, actionType, expectedResult);
    }

    [Fact]
    public async Task AddPipeline_WithCustomSpecifiedName_PipelineWithTheSameIdentifier_ReplaceFirst()
    {
        var name = "pipenineName";
        var sp = Factory.CreateServiceProvider(c => c
            .AddPipeline(c => true, p =>
                {
                    p.Use<FakeMiddleware1>();
                },
                name)
            .AddPipeline(c => true, p =>
                {
                    p.Use<FakeMiddleware2>();
                },
                name)
        );
        await AssertAction(sp, typeof(FakeAction1), Result2);
    }

    [Fact]
    public async Task AddPipelineForAction_PipelineWithTheSameIdentifier_ReplaceFirst()
    {
        var sp = Factory.CreateServiceProvider(c => c
            .AddPipelineForAction<FakeAction1>(p =>
            {
                p.Use<FakeMiddleware1>();
            })
            .AddPipelineForAction<FakeAction1>(p =>
            {
                p.Use<FakeMiddleware2>();
            })
        );
        await AssertAction(sp, typeof(FakeAction1), Result2);
    }

    private async Task AssertAction(IServiceProvider services, Type actionType, string expectedResult)
    {
        var response = await Dispatch(services, actionType);
        var containsResult = response.Results.Any(r => r is string s && s == expectedResult);
        Assert.True(containsResult);
    }

    private static async Task<IMediatorResponse> Dispatch(IServiceProvider services, Type actionType)
    {
        var mediator = services.GetRequiredService<IMediator>();
        var action = (IMediatorAction)Activator.CreateInstance(actionType)!;
        var response = await mediator.Dispatch(action);
        return response;
    }

    public class FakeAction1 : IMediatorAction
    {
    }

    public class FakeAction2 : IMediatorAction
    {
    }

    private class FakeMiddleware1 : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddResult(Result1);
            await next(context);
        }
    }

    private class FakeMiddleware2 : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddResult(Result2);
            await next(context);
        }
    }
}