using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Tests.Fakes;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests;

/// <summary>
/// End-to-end regression coverage for the structural claim behind
/// <see cref="MediatorContextExtensions.SetResponseStatusCodeHint"/>: a hint set on a nested
/// <see cref="MediatorContext"/> (<see cref="MediatorContext.IsNested"/> is <c>true</c>) never leaks into the root
/// HTTP response - it is dropped at the point it would be set, mirroring the "root vs. nested" safety
/// <see cref="IMediatorHttpResult"/> already gives for full response bodies.
/// </summary>
public class MediatorMiddleware_NestedStatusCodeHintTests
{
    private const string _parentRequest =
        "{\"$type\":\"Pipaslot.Mediator.Http.Tests.StatusHintParentAction, Pipaslot.Mediator.Http.Tests\"}";

    [Fact]
    public async Task WillApplyRootHint_AndIgnoreHintSetByNestedCall()
    {
        var services = CreateServiceProvider();
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_parentRequest), services, response);
        await sut.Invoke(context);

        // Root middleware hints 201; the nested child action also hints (409), but that call is nested
        // (Depth > 1), so its hint must never reach the root HTTP response.
        Assert.Equal(201, response.StatusCode);
    }

    private static IServiceProvider CreateServiceProvider()
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddMediatorServer(o => o.DeserializeOnlyCredibleActionTypes = false)
            .AddActions([typeof(StatusHintParentAction), typeof(StatusHintChildAction)])
            .AddHandlers([typeof(StatusHintParentActionHandler), typeof(StatusHintChildActionHandler)])
            .Use<StatusHintMiddleware>();
        collection.AddScoped<MediatorMiddleware>();
        collection.AddScoped<RequestDelegate>(_ => _ => Task.CompletedTask);
        return collection.BuildServiceProvider();
    }
}

internal class StatusHintParentAction : IMessage;

internal class StatusHintParentActionHandler(IMediator mediator) : IMessageHandler<StatusHintParentAction>
{
    public Task Handle(StatusHintParentAction action, CancellationToken cancellationToken)
    {
        return mediator.DispatchUnhandled(new StatusHintChildAction(), cancellationToken);
    }
}

internal class StatusHintChildAction : IMessage;

internal class StatusHintChildActionHandler : IMessageHandler<StatusHintChildAction>
{
    public Task Handle(StatusHintChildAction action, CancellationToken cancellationToken) => Task.CompletedTask;
}

/// <summary>
/// Stands in for a validation-style middleware (e.g. the <c>ValidationMediatorMiddleware</c> anti-pattern this
/// feature replaces) that hints a status code for every action it sees, regardless of nesting.
/// </summary>
internal class StatusHintMiddleware : IMediatorMiddleware
{
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        switch (context.Action)
        {
            case StatusHintParentAction:
                context.SetResponseStatusCodeHint(201);
                break;
            case StatusHintChildAction:
                context.SetResponseStatusCodeHint(409);
                break;
        }

        return next(context);
    }
}
