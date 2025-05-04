using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
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
        @"{""Success"":true,""Results"":[{""$type"":""Pipaslot.Mediator.Benchmarks." + nameof(MediatorClient) + "+" + nameof(TestResponse) +
        @", Pipaslot.Mediator.Benchmarks"",""Message"":""Hello World""}]}";

    private const string _apiEndpoint = "/api/custom-api-operation";
    private const string _apiResponse = @"{""Message"":""Hello World""}";

    [GlobalSetup]
    public void GlobalSetup()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        // Setup mock HttpClient
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.LocalPath.StartsWith(MediatorConstants.Endpoint)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(_mediatorResponse) });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(m => m.RequestUri.LocalPath.StartsWith(_apiEndpoint)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(() => new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent(_apiResponse) });

        _httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

        services.AddMediatorClient();
        services.AddSingleton(_ => _httpClient);

        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    public record TestRequest : IRequest<TestResponse>;

    public record TestResponse(string Message);


    [Benchmark(Baseline = true)]
    public async Task RawHttpClient()
    {
        var httpResponse = await _httpClient.PostAsJsonAsync(_apiEndpoint, new TestRequest());
        var result = await httpResponse.Content.ReadFromJsonAsync<TestResponse>();

        if (result is null || result.Message != "Hello World")
        {
            throw new Exception("Unexpected response: " + result);
        }
    }

    [Benchmark]
    public async Task Mediator()
    {
        var response = await _mediator.Execute(new TestRequest());
        if (response.Failure || response.Result.Message != "Hello World")
        {
            throw new Exception("Unexpected response:" + response.Result);
        }
    }
}