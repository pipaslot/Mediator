using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Benchmarks.Actions;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure an overall mediator performance for in-process use-case without networking
/// </summary>
[MemoryDiagnoser]
public class MediatorCore
{
    private IMediator _mediator = null!;
    private readonly MessageAction _message = new();
    private readonly RequestAction _request = new("Hello World");
    private readonly RequestAction1 _authenticatedRequest = new("Hello World");

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddMediator()
            .AddActions([typeof(MessageAction), typeof(RequestAction), typeof(RequestAction1)])
            .AddHandlers([typeof(MessageActionHandler), typeof(RequestActionHandler), typeof(RequestAction1Handler)])
            .UseWhenAction<RequestAction1>(m => m.UseAuthorization());

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public Task Message()
    {
        return _mediator.DispatchUnhandled(_message);
    }

    [Benchmark]
    public Task Request()
    {
        return _mediator.ExecuteUnhandled(_request);
    }

    [Benchmark]
    public Task RequestWithAuthentication()
    {
        return _mediator.ExecuteUnhandled(_authenticatedRequest);
    }
}