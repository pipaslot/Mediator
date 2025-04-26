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
    private MediatorContext _notification = null!;
    private MediatorContext _request = null!;
    private Task Next(MediatorContext context) => Task.CompletedTask;
    private readonly HandlerExecutionMiddleware _executionMiddleware = new ();

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddMediator()
            .AddActions([typeof(NotificationAction),typeof(RequestAction)])
            .AddHandlers([typeof(NotificationActionHandler), typeof(RequestActionHandler)]);

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
    private class NotificationActionHandler : IMediatorHandler<NotificationAction>
    {
        public Task Handle(NotificationAction action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private record RequestAction(string Message) : IMediatorAction<string>;
    private class RequestActionHandler : IMediatorHandler<RequestAction, string>
    {
        public Task<string> Handle(RequestAction action, CancellationToken cancellationToken)
        {
            return Task.FromResult(action.Message);
        }
    }
}