using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests.Middlewares;

public class HttpClientExecutionMiddlewareTests
{
    private static readonly string _actionIdentifier = typeof(NopRequest).ToString();

    [Fact]
    public async Task Execute_PostsSerializedActionToConfiguredEndpoint_AndReturnsDeserializedResult()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = CreateHandler(HttpStatusCode.OK, "irrelevant-body", req => capturedRequest = req);
        var (mediator, serializer) = CreateSut(handler);
        serializer.DeserializeResponse<object>(Arg.Any<string>())
            .Returns(new MediatorResponse<object>(true, ["hello"]));

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
        var handler = CreateHandler(HttpStatusCode.OK, "irrelevant-body");
        var (mediator, serializer) = CreateSut(handler);
        serializer.DeserializeResponse<object>(Arg.Any<string>())
            .Returns(new MediatorResponse<object>(false, ["validation failed"]));

        var response = await mediator.Execute(new NopRequest());

        Assert.False(response.Success);
        Assert.Contains("validation failed", response.Results);
    }

    [Fact]
    public async Task Execute_WhenSerializedRequestContainsStreams_SendsMultipartFormDataContent()
    {
        HttpRequestMessage? capturedRequest = null;
        var handler = CreateHandler(HttpStatusCode.OK, "irrelevant-body", req => capturedRequest = req);
        var (mediator, serializer) = CreateSut(handler);

        using var fileStream = new MemoryStream([1, 2, 3]);
        serializer.SerializeRequest(Arg.Any<IMediatorAction>())
            .Returns(new SerializedRequest("{}", [new StreamContract("file-1", fileStream)]));
        serializer.DeserializeResponse<object>(Arg.Any<string>())
            .Returns(new MediatorResponse<object>(true, ["done"]));

        var response = await mediator.Execute(new NopRequest());

        Assert.True(response.Success);
        Assert.NotNull(capturedRequest);
        Assert.IsType<MultipartFormDataContent>(capturedRequest!.Content);
    }

    [Fact]
    public async Task Execute_WhenHttpClientThrowsNonCancellationException_ReturnsFailedResponseWrappingRuntimeError()
    {
        var handler = CreateThrowingHandler(new HttpRequestException("boom"));
        var (mediator, _) = CreateSut(handler);

        var response = await mediator.Execute(new NopRequest());

        Assert.False(response.Success);
        var message = response.GetErrorMessage();
        Assert.Contains($"Error occured when executed action {_actionIdentifier}", message);
        Assert.Contains("boom", message);
    }

    [Fact]
    public async Task Execute_WhenHttpClientThrowsOperationCanceledException_ReturnsGenericErrorViaCoreBoundary()
    {
        // The middleware explicitly rethrows cancellation exceptions instead of routing them through
        // ProcessRuntimeError/CreateErrorResponse, so the message must NOT be wrapped with the runtime-error prefix.
        var handler = CreateThrowingHandler(new OperationCanceledException("cancelled"));
        var (mediator, _) = CreateSut(handler);

        var response = await mediator.Execute(new NopRequest());

        Assert.False(response.Success);
        var message = response.GetErrorMessage();
        // The middleware rethrows the cancellation exception unwrapped, but the
        // core Execute boundary is the single catching point for the whole library - it still applies the generic
        // safe-by-default fallback for this unmapped exception rather than leaking its own message.
        Assert.Equal(Mediator.GenericErrorMessage, message);
        Assert.DoesNotContain("Error occured when executed action", message);
    }

    [Fact]
    public async Task Execute_WhenServerReturnsNonSuccessStatusCodeButValidResponse_IgnoresStatusCodeAndPropagatesResult()
    {
        // The middleware explicitly does not check the HTTP status code - it is up to the server to decide which
        // status code to send for a failed action, as long as the body still parses into a Mediator response.
        var handler = CreateHandler(HttpStatusCode.InternalServerError, "irrelevant-body");
        var (mediator, serializer) = CreateSut(handler);
        serializer.DeserializeResponse<object>(Arg.Any<string>())
            .Returns(new MediatorResponse<object>(true, ["hello"]));

        var response = await mediator.Execute(new NopRequest());

        Assert.True(response.Success);
        Assert.Equal("hello", response.Result);
    }

    [Fact]
    public async Task Execute_WhenDeserializedResponseIsNull_ReturnsFailedResponseWithNoDataReceivedMessage()
    {
        var handler = CreateHandler(HttpStatusCode.OK, "irrelevant-body");
        var (mediator, serializer) = CreateSut(handler);
        serializer.DeserializeResponse<object>(Arg.Any<string>())
            .Returns((IMediatorResponse<object>)null!);

        var response = await mediator.Execute(new NopRequest());

        Assert.False(response.Success);
        var message = response.GetErrorMessage();
        Assert.Contains($"No data received for action {_actionIdentifier}", message);
    }

    [Fact]
    public async Task Execute_WhenResponseBodyFailsToDeserialize_ReturnsFailedResponseWithParsingErrorMessage()
    {
        var handler = CreateHandler(HttpStatusCode.BadGateway, "not-json");
        var (mediator, serializer) = CreateSut(handler);
        serializer.DeserializeResponse<object>(Arg.Any<string>())
            .Returns(_ => throw new InvalidOperationException("bad json"));

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
        var (_, formatter, serializer) = CreateSutWithFormatter(new FakeHttpMessageHandler((_, _) => Task.FromResult(new HttpResponseMessage())));
        serializer.SerializeRequest(Arg.Any<IMediatorAction>())
            .Returns(new SerializedRequest("hello%20world", Array.Empty<StreamContract>()));

        var url = formatter.FormatHttpGet(new NopRequest());

        Assert.Equal("/_mediator/request?action=hello world", url);
    }

    /// <summary>
    /// Wires an <see cref="IMediator"/> around the given HTTP handler with a serializer substitute that already has the
    /// common <c>SerializeRequest</c> setup applied - individual tests only need to add the
    /// <c>DeserializeResponse</c>/<c>SerializeRequest</c> setup that is actually the point of that test.
    /// </summary>
    private static (IMediator Mediator, IContractSerializer Serializer) CreateSut(HttpMessageHandler handler)
    {
        var (mediator, _, serializer) = CreateSutWithFormatter(handler);
        return (mediator, serializer);
    }

    private static (IMediator Mediator, IMediatorUrlFormatter Formatter, IContractSerializer Serializer) CreateSutWithFormatter(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var serializer = Substitute.For<IContractSerializer>();
        serializer.SerializeRequest(Arg.Any<IMediatorAction>())
            .Returns(new SerializedRequest("{}", Array.Empty<StreamContract>()));

        var provider = CreateServiceProvider(httpClient, serializer);
        return (provider.GetRequiredService<IMediator>(), provider.GetRequiredService<IMediatorUrlFormatter>(), serializer);
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

    private static FakeHttpMessageHandler CreateHandler(HttpStatusCode statusCode, string content, Action<HttpRequestMessage>? onRequest = null)
    {
        return new FakeHttpMessageHandler((req, _) =>
        {
            onRequest?.Invoke(req);
            return Task.FromResult(new HttpResponseMessage(statusCode) { Content = new StringContent(content) });
        });
    }

    private static FakeHttpMessageHandler CreateThrowingHandler(Exception exception)
    {
        return new FakeHttpMessageHandler((_, _) => Task.FromException<HttpResponseMessage>(exception));
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

    public class NopRequest : IRequest<string>;
}
