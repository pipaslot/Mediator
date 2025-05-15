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
    private IMediator _mediatorWithContextAccessor = null!;
    private IMediator _mediatorMinimal = null!;
    private readonly MessageAction _message = new();
    private readonly RequestAction _request = new("Hello World");
    private readonly RequestAction1 _authenticatedRequest = new("Hello World");

    [GlobalSetup]
    public void GlobalSetup()
    {
        _mediatorWithContextAccessor = CreateMediator(true);
        _mediatorMinimal = CreateMediator(false);
    }

    private IMediator CreateMediator(bool addContextAccessor)
    {
        var services = new ServiceCollection();
        services.AddMediator(addContextAccessor)
            .AddActions([typeof(MessageAction), typeof(RequestAction), typeof(RequestAction1)])
            .AddHandlers([typeof(MessageActionHandler), typeof(RequestActionHandler), typeof(RequestAction1Handler)])
            .UseWhenAction<RequestAction1>(m => m.UseAuthorization());

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IMediator>();
    }

    [Benchmark(Baseline = true)]
    public Task Message()
    {
        return _mediatorMinimal.DispatchUnhandled(_message);
    }

    [Benchmark]
    public Task Request()
    {
        return _mediatorMinimal.ExecuteUnhandled(_request);
    }
    
    [Benchmark]
    public Task MessageWithContextAccessor()
    {
        return _mediatorWithContextAccessor.DispatchUnhandled(_message);
    }

    [Benchmark]
    public Task RequestWithContextAccessor()
    {
        return _mediatorWithContextAccessor.ExecuteUnhandled(_request);
    }

    [Benchmark]
    public Task RequestWithAuthentication()
    {
        return _mediatorMinimal.ExecuteUnhandled(_authenticatedRequest);
    }
}