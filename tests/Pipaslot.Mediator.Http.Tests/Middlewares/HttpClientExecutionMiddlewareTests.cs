using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Middlewares;

public class HttpClientExecutionMiddlewareTests
{
    private static readonly string _actionIdentifier = typeof(NopRequest).ToString();

    [Fact]
    public async Task Execute_PostsSerializedActionToConfiguredEndpoint_AndReturnsDeserializedResult()
    {
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK, "irrelevant-body", req => capturedRequest = req);
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

        var serializerMock = new Mock<IContractSerializer>();
        serializerMock.Setup(x => x.SerializeRequest(It.IsAny<IMediatorAction>()))
            .Returns(new SerializedRequest("{}", Array.Empty<StreamContract>()));
        serializerMock.Setup(x => x.DeserializeResponse<object>(It.IsAny<string>()))
            .Returns(new MediatorResponse<object>(true, ["hello"]));

        var provider = CreateServiceProvider(httpClient, serializerMock.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Execute(new NopRequest());

        Assert.True(response.Success);
        Assert.Equal("hello", response.Result);
        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal($"/_mediator/request?type={_actionIdentifier}", capturedRequest.RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task Execute_WhenServerReturnsUnsuccessfulButValidResponse_PropagatesFailureWithoutRuntimeOrParsingError()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK, "irrelevant-body");
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

        var serializerMock = new Mock<IContractSerializer>();
        serializerMock.Setup(x => x.SerializeRequest(It.IsAny<IMediatorAction>()))
            .Returns(new SerializedRequest("{}", Array.Empty<StreamContract>()));
        serializerMock.Setup(x => x.DeserializeResponse<object>(It.IsAny<string>()))
            .Returns(new MediatorResponse<object>(false, ["validation failed"]));

        var provider = CreateServiceProvider(httpClient, serializerMock.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Execute(new NopRequest());

        Assert.False(response.Success);
        Assert.Contains("validation failed", response.Results);
    }

    [Fact]
    public async Task Execute_WhenSerializedRequestContainsStreams_SendsMultipartFormDataContent()
    {
        HttpRequestMessage? capturedRequest = null;
        var handlerMock = CreateHandlerMock(HttpStatusCode.OK, "irrelevant-body", req => capturedRequest = req);
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

        using var fileStream = new MemoryStream([1, 2, 3]);
        var serializerMock = new Mock<IContractSerializer>();
        serializerMock.Setup(x => x.SerializeRequest(It.IsAny<IMediatorAction>()))
            .Returns(new SerializedRequest("{}", [new StreamContract("file-1", fileStream)]));
        serializerMock.Setup(x => x.DeserializeResponse<object>(It.IsAny<string>()))
            .Returns(new MediatorResponse<object>(true, ["done"]));

        var provider = CreateServiceProvider(httpClient, serializerMock.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Execute(new NopRequest());

        Assert.True(response.Success);
        Assert.NotNull(capturedRequest);
        Assert.IsType<MultipartFormDataContent>(capturedRequest!.Content);
    }

    [Fact]
    public async Task Execute_WhenHttpClientThrowsNonCancellationException_ReturnsFailedResponseWrappingRuntimeError()
    {
        var handlerMock = CreateThrowingHandlerMock(new HttpRequestException("boom"));
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

        var serializerMock = new Mock<IContractSerializer>();
        serializerMock.Setup(x => x.SerializeRequest(It.IsAny<IMediatorAction>()))
            .Returns(new SerializedRequest("{}", Array.Empty<StreamContract>()));

        var provider = CreateServiceProvider(httpClient, serializerMock.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Execute(new NopRequest());

        Assert.False(response.Success);
        var message = response.GetErrorMessage();
        Assert.Contains($"Error occured when executed action {_actionIdentifier}", message);
        Assert.Contains("boom", message);
    }

    [Fact]
    public async Task Execute_WhenHttpClientThrowsOperationCanceledException_PropagatesWithoutWrapping()
    {
        // The middleware explicitly rethrows cancellation exceptions instead of routing them through
        // ProcessRuntimeError/CreateErrorResponse, so the message must NOT be wrapped with the runtime-error prefix.
        var handlerMock = CreateThrowingHandlerMock(new OperationCanceledException("cancelled"));
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

        var serializerMock = new Mock<IContractSerializer>();
        serializerMock.Setup(x => x.SerializeRequest(It.IsAny<IMediatorAction>()))
            .Returns(new SerializedRequest("{}", Array.Empty<StreamContract>()));

        var provider = CreateServiceProvider(httpClient, serializerMock.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Execute(new NopRequest());

        Assert.False(response.Success);
        var message = response.GetErrorMessage();
        Assert.Equal("cancelled", message);
        Assert.DoesNotContain("Error occured when executed action", message);
    }

    [Fact]
    public async Task Execute_WhenResponseBodyFailsToDeserialize_ReturnsFailedResponseWithParsingErrorMessage()
    {
        var handlerMock = CreateHandlerMock(HttpStatusCode.BadGateway, "not-json");
        var httpClient = new HttpClient(handlerMock.Object) { BaseAddress = new Uri("http://localhost/") };

        var serializerMock = new Mock<IContractSerializer>();
        serializerMock.Setup(x => x.SerializeRequest(It.IsAny<IMediatorAction>()))
            .Returns(new SerializedRequest("{}", Array.Empty<StreamContract>()));
        serializerMock.Setup(x => x.DeserializeResponse<object>(It.IsAny<string>()))
            .Throws(new InvalidOperationException("bad json"));

        var provider = CreateServiceProvider(httpClient, serializerMock.Object);
        var mediator = provider.GetRequiredService<IMediator>();

        var response = await mediator.Execute(new NopRequest());

        Assert.False(response.Success);
        var message = response.GetErrorMessage();
        Assert.Contains($"Can not deserialize response for action {_actionIdentifier}", message);
        Assert.Contains("bad json", message);
        Assert.Contains("502", message);
        Assert.Contains("BadGateway", message);
    }

    [Fact]
    public void FormatHttpGet_AppendsUrlDecodedSerializedActionToEndpoint()
    {
        var serializerMock = new Mock<IContractSerializer>();
        serializerMock.Setup(x => x.SerializeRequest(It.IsAny<IMediatorAction>()))
            .Returns(new SerializedRequest("hello%20world", Array.Empty<StreamContract>()));

        var httpClient = new HttpClient(new Mock<HttpMessageHandler>().Object) { BaseAddress = new Uri("http://localhost/") };
        var provider = CreateServiceProvider(httpClient, serializerMock.Object);
        var formatter = provider.GetRequiredService<IMediatorUrlFormatter>();

        var url = formatter.FormatHttpGet(new NopRequest());

        Assert.Equal("/_mediator/request?action=hello world", url);
    }

    private static ServiceProvider CreateServiceProvider(HttpClient httpClient, IContractSerializer serializer)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(httpClient);
        services.AddMediatorClient();
        services.AddSingleton(serializer);
        return services.BuildServiceProvider();
    }

    private static Mock<HttpMessageHandler> CreateHandlerMock(HttpStatusCode statusCode, string content, Action<HttpRequestMessage>? onRequest = null)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, _) => onRequest?.Invoke(req))
            .ReturnsAsync(() => new HttpResponseMessage(statusCode) { Content = new StringContent(content) });
        return handlerMock;
    }

    private static Mock<HttpMessageHandler> CreateThrowingHandlerMock(Exception exception)
    {
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(exception);
        return handlerMock;
    }
}
