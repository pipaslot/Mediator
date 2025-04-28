using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Pipaslot.Mediator.Http;
using System.Net;

namespace Pipaslot.Mediator.Benchmarks;

/// <summary>
/// Measure performance of the client side part with mocked HTTP networking
/// </summary>
[MemoryDiagnoser]
public class ClientSideBenchmarks
{
    private IMediator _mediator = null!;
    private HttpClient _httpClient = null!;

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
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK, Content = new StringContent("{\"Success\":true,\"Results\":[{\"$type\":\"System.String, System.Private.CoreLib\",\"Value\":\"Hello World\"}]}")
            });

        _httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

        services.AddMediatorClient();
        services.AddSingleton(_ => _httpClient);

        var serviceProvider = services.BuildServiceProvider();
        _mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    public record TestRequest : IRequest<string>;

    [Benchmark]
    public async Task Execute()
    {
        var response = await _mediator.Execute(new TestRequest());
        if (response.Failure || response.Result != "Hello World")
        {
            throw new Exception("Unexpected response:" +response.Result);       
        }
    }
}