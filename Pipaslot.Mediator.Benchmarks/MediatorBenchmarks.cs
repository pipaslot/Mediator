using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Benchmarks.Actions;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure an overall mediator performance when invoked on server side
/// </summary>
[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private IMediator _mediator = null!;
    private readonly MessageAction _message = new();
    private readonly RequestAction _request = new("Hello World");

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddMediator()
            .AddActions([typeof(MessageAction), typeof(RequestAction)])
            .AddHandlers([typeof(MessageActionHandler), typeof(RequestActionHandler)]);

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public Task Execute()
    {
        return _mediator.ExecuteUnhandled(_request);
    }

    [Benchmark]
    public Task Dispatch()
    {
        return _mediator.DispatchUnhandled(_message);
    }
}