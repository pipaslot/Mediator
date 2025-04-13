using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Benchmark middleware resolving and running handlers
/// </summary>
[MemoryDiagnoser]
public class HandlerExecutionMiddlewareBenchmarks
{
    private readonly MediatorContext _notification;
    private readonly MediatorContext _request;
    private Task Next(MediatorContext context) => Task.CompletedTask;
    private readonly HandlerExecutionMiddleware _executionMiddleware = new ();

    public HandlerExecutionMiddlewareBenchmarks()
    {
        var services = new ServiceCollection();
        services.AddMediator()
            .AddActionsFromAssemblyOf<MediatorBenchmarks>()
            .AddHandlersFromAssemblyOf<MediatorBenchmarks>();

        var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();
        var contextAccessor = provider.GetRequiredService<IMediatorContextAccessor>();
        _notification = new(mediator, contextAccessor, provider,
            new NotificationAction("Hello world"), CancellationToken.None, null, null);
        _request = new(mediator, contextAccessor, provider,
            new RequestAction("Hello world"), CancellationToken.None, null, null);
    }

    [Benchmark]
    public Task Notification()
    {
        return _executionMiddleware.Invoke(_notification, Next);
    }
    
    [Benchmark]
    public Task Request()
    {
        return _executionMiddleware.Invoke(_request, Next);
    }

    private record NotificationAction(string Message) : IMediatorAction;

    private record RequestAction(string Message) : IMediatorAction<string>;
}