using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Benchmarks.Actions;
using Pipaslot.Mediator.Http;
using System.Net;
using System.Net.Http.Json;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure performance of the client side part with mocked HTTP networking
/// </summary>
[MemoryDiagnoser]
public class MediatorClient
{
    private IMediator _mediator = null!;
    private HttpClient _httpClient = null!;

    private const string _mediatorResponse =
        @"{""Success"":true,""Results"":[{""$type"":""Pipaslot.Mediator.Benchmarks.Actions." +
        nameof(RequestActionResult) +
        @", Pipaslot.Mediator.Benchmarks"",""Message"":""Hello World""}]}";

    private const string _apiEndpoint = "/api/custom-api-operation";
    private const string _apiResponse = @"{""Message"":""Hello World""}";

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Setup fake HttpClient
        var handler = new FakeHttpMessageHandler((request, _) =>
        {
            var response = request.RequestUri!.LocalPath.StartsWith(MediatorConstants.Endpoint)
                ? new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(_mediatorResponse) }
                : new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(_apiResponse) };
            return Task.FromResult(response);
        });

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };

        services.AddMediatorClient(o => o.AddContextAccessor = false);
        services.AddSingleton(_ => _httpClient);

        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    [Benchmark(Baseline = true)]
    public async Task RawHttpClient()
    {
        var httpResponse = await _httpClient.PostAsJsonAsync(_apiEndpoint, new RequestAction("Hello World"));
        var result = await httpResponse.Content.ReadFromJsonAsync<RequestActionResult>();

        if (result is null || result.Message != "Hello World")
        {
            throw new Exception("Unexpected response: " + result);
        }
    }

    [Benchmark]
    public async Task Mediator()
    {
        var response = await _mediator.Execute(new RequestAction("Hello World"));
        if (response.Failure || response.Result.Message != "Hello World")
        {
            throw new Exception("Unexpected response:" + response.Result);
        }
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