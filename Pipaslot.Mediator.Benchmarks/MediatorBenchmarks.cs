using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure an overall mediator performance
/// </summary>
[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private readonly IMediator _mediator;
    private readonly Pinged _message = new();
    private readonly Ping _request = new("Hello World");

    public MediatorBenchmarks()
    {
        var services = new ServiceCollection();
        services.AddMediator()
            .AddActions([typeof(Ping), typeof(Pinged)])
            .AddHandlers([typeof(PingHandler), typeof(PingedHandler)]);

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

    public record Ping(string Message) : IRequest<string>;

    public class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Message);
        }
    }

    public class Pinged : IMessage;

    public class PingedHandler : IMessageHandler<Pinged>
    {
        public Task Handle(Pinged notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}