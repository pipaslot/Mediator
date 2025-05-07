using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Http;
using System.Reflection;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure performance of the client side application initialization
/// </summary>
[MemoryDiagnoser]
public class MediatorClientStartup
{
    private readonly Assembly _assembly = typeof(Program).Assembly;

    [Benchmark(Baseline = true)]
    public void MediatorWithNoAction()
    {
        var services = new ServiceCollection();
        services.AddMediatorClient();
    }

    [Benchmark]
    public void MediatorWith502Actions()
    {
        var services = new ServiceCollection();
        services.AddMediatorClient()
            .AddActionsFromAssembly(_assembly);
    }
}