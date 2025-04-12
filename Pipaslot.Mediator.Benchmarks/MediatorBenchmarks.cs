using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator.Benchmarks;

[MemoryDiagnoser]
public class MediatorBenchmarks
{
    private IMediator _mediator = null!;
    private readonly Pinged _message = new();
    private readonly Ping _request = new() { Message = "Hello World" };

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddMediator()
            .AddActionsFromAssemblyOf<MediatorBenchmarks>()
            .AddHandlersFromAssemblyOf<MediatorBenchmarks>();

        var provider = services.BuildServiceProvider();

        _mediator = provider.GetRequiredService<IMediator>();
    }

    [Benchmark]
    public Task Execute()
    {
        return _mediator.ExecuteUnhandled(_request);
    }
    
    [Benchmark]
    public Task Dispath()
    {
        return _mediator.DispatchUnhandled(_message);
    }

    public class Ping : IRequest<string>
    {
        public string Message { get; set; } = string.Empty;
    }

    public class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.Message);
        }
    }

    public class Pinged : IMessage
    {
    }

    public class PingedHandler : IMessageHandler<Pinged>
    {
        public Task Handle(Pinged notification, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}