using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Benchmarks.Actions;
using Pipaslot.Mediator.Http;
using System.Net;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure performance impact of growing application to estimate/track the potential impact on the client side startup time
/// </summary>
[MemoryDiagnoser]
public class MediatorClientStartup
{
    private HttpClient _httpClient = null!;

    [GlobalSetup]
    public void GlobalSetup()
    {
        // Setup fake HttpClient
        var handler = new FakeHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(
                @"{""Success"":true,""Results"":[{""$type"":""Pipaslot.Mediator.Benchmarks.Actions." +
                nameof(RequestActionResult) +
                @", Pipaslot.Mediator.Benchmarks"",""Message"":""Hello World""}]}")
        }));

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
    }

    [Benchmark(Baseline = true)]
    public Task SingleAction()
    {
        var services = CreateServiceCollection();
        services.AddMediatorClient(o =>
            {
                o.DeserializeOnlyCredibleResultTypes = true;
                o.AddContextAccessor = false;
            })
            .AddActions([typeof(RequestAction)]);
        
        return RunAction(services);
    }

    [Benchmark]
    public Task Containing502Actions()
    {
        var services = CreateServiceCollection();
        services.AddMediatorClient(o=> o.DeserializeOnlyCredibleResultTypes = true)
            .AddActionsFromAssembly(typeof(Program).Assembly);
        
        return RunAction(services);
    }
    
    private ServiceCollection CreateServiceCollection()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(_ => _httpClient);
        return services;
    }

    private async Task RunAction(IServiceCollection services)
    {
        var mediator = services.BuildServiceProvider().GetRequiredService<IMediator>();
        await mediator.ExecuteUnhandled(new RequestAction("Hello World"));
    }

    /// <summary>
    /// <see cref="HttpMessageHandler.SendAsync"/> is protected, so it cannot be reached through an interface-based
    /// substitute - overriding it directly on a throwaway subclass is simpler than reflecting into a protected member.
    /// </summary>
    private sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> handler) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => handler(request, cancellationToken);
    }
}