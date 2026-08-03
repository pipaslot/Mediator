using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Tests.Fakes;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests;

/// <summary>
/// End-to-end regression coverage for the structural claim behind <see cref="IMediatorHttpResult"/>:
/// a nested mediator call's result never leaks into the root HTTP response unless the calling handler
/// explicitly forwards it as its own result. No <c>IsNested</c> check is required anywhere.
/// </summary>
public class MediatorMiddleware_NestedHttpResultTests
{
    private const string _parentRequest =
        "{\"$type\":\"Pipaslot.Mediator.Http.Tests.NestedResultParentAction, Pipaslot.Mediator.Http.Tests\"}";

    [Fact]
    public async Task WillApplyHttpResult_WhenForwardedByParentHandler()
    {
        var childResult = new FakeHttpResult();
        var services = CreateServiceProvider(childResult, typeof(ForwardingParentActionHandler));
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_parentRequest), services, response);
        await sut.Invoke(context);

        Assert.True(childResult.Applied);
        Assert.Equal(string.Empty, response.ContentType);
    }

    [Fact]
    public async Task WillWriteJson_WhenParentHandlerDoesNotForwardNestedResult()
    {
        var childResult = new FakeHttpResult();
        var services = CreateServiceProvider(childResult, typeof(DiscardingParentActionHandler));
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_parentRequest), services, response);
        await sut.Invoke(context);

        Assert.False(childResult.Applied);
        Assert.Equal("application/json; charset=utf-8", response.ContentType);
    }

    private static IServiceProvider CreateServiceProvider(FakeHttpResult childResult, Type parentHandlerType)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddSingleton(childResult);
        collection.AddMediatorServer(o => o.DeserializeOnlyCredibleActionTypes = false)
            .AddActions([typeof(NestedResultParentAction), typeof(NestedResultChildAction)])
            .AddHandlers([parentHandlerType, typeof(NestedResultChildActionHandler)]);
        collection.AddScoped<MediatorMiddleware>();
        collection.AddScoped<RequestDelegate>(_ => _ => Task.CompletedTask);
        return collection.BuildServiceProvider();
    }
}

internal class NestedResultParentAction : IMediatorAction<object>;

internal class ForwardingParentActionHandler(IMediator mediator) : IMediatorHandler<NestedResultParentAction, object>
{
    public async Task<object> Handle(NestedResultParentAction action, CancellationToken cancellationToken)
    {
        return await mediator.ExecuteUnhandled(new NestedResultChildAction(), cancellationToken);
    }
}

internal class DiscardingParentActionHandler(IMediator mediator) : IMediatorHandler<NestedResultParentAction, object>
{
    public async Task<object> Handle(NestedResultParentAction action, CancellationToken cancellationToken)
    {
        await mediator.ExecuteUnhandled(new NestedResultChildAction(), cancellationToken);
        return "plain-result";
    }
}

internal class NestedResultChildAction : IMediatorAction<IMediatorHttpResult>;

internal class NestedResultChildActionHandler(FakeHttpResult result) : IMediatorHandler<NestedResultChildAction, IMediatorHttpResult>
{
    public Task<IMediatorHttpResult> Handle(NestedResultChildAction action, CancellationToken cancellationToken)
    {
        return Task.FromResult<IMediatorHttpResult>(result);
    }
}
