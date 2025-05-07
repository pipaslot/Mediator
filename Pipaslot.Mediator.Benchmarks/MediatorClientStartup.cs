using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Benchmarks.Actions;
using Pipaslot.Mediator.Http;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure performance impact of growing application to estimate/track the potential impact on the client side startup time
/// </summary>
[MemoryDiagnoser]
public class MediatorClientStartup
{
    [Benchmark(Baseline = true)]
    public void SingleAction()
    {
        var services = new ServiceCollection();
        services.AddMediatorClient()
            .AddActions([typeof(RequestAction)]);
        
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        mediator.Execute(new RequestAction("Hello World"));
    }

    [Benchmark]
    public void Containing502Actions()
    {
        var services = new ServiceCollection();
        services.AddMediatorClient()
            .AddActionsFromAssembly(typeof(Program).Assembly);
        
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        mediator.Execute(new RequestAction("Hello World"));
    }
}