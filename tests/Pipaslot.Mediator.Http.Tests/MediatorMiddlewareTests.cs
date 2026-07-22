using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Tests.Fakes;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests;

public class MediatorMiddlewareTests
{
    private const string _request =
        "{\"$type\":\"Pipaslot.Mediator.Tests.ValidActions.NopRequest, Pipaslot.Mediator.Tests.ValidActions\"}";

    private const string _message =
        "{\"$type\":\"Pipaslot.Mediator.Tests.ValidActions.NopMessage, Pipaslot.Mediator.Tests.ValidActions\"}";

    [Fact]
    public async Task PostMessageWillBePropagatedToMediator()
    {
        await ExecuteMessage(new FakePostRequest(_message));
    }

    [Fact]
    public async Task PostRequestWillBePropagatedToMediator()
    {
        await ExecuteRequest(new FakePostRequest(_request));
    }

    [Fact]
    public async Task GetMessageWillBePropagatedToMediator()
    {
        await ExecuteMessage(new FakeGetRequest(_message));
    }

    [Fact]
    public async Task GetRequestWillBePropagatedToMediator()
    {
        await ExecuteRequest(new FakeGetRequest(_request));
    }

    [Fact]
    public async Task WillNotWriteResponse_WhenResponseAlreadyHasStarted()
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, Array.Empty<object>()));
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Dispatch(It.IsAny<IMediatorAction>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediatorMock);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse(hasStarted: true);
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        // Default JSON serialization must be skipped once something else already wrote to the response
        Assert.Equal(string.Empty, response.ContentType);
    }

    [Fact]
    public async Task WillSetErrorStatusCode_WhenMediatorResponseFailed()
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(false, Array.Empty<object>()));
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Dispatch(It.IsAny<IMediatorAction>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediatorMock);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.Equal(MediatorConstants.ErrorHttpStatusCode, response.StatusCode);
    }

    [Fact]
    public async Task WillPreserveStatusCode_WhenAlreadyChangedByMiddleware()
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(false, Array.Empty<object>()));
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Dispatch(It.IsAny<IMediatorAction>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediatorMock);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse { StatusCode = (int)HttpStatusCode.BadRequest };
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.Equal((int)HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WillApplyHttpResult_InsteadOfWritingJson()
    {
        var httpResult = new FakeHttpResult();
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, [httpResult]));
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Dispatch(It.IsAny<IMediatorAction>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediatorMock);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.True(httpResult.Applied);
        Assert.Equal(string.Empty, response.ContentType);
    }

    [Fact]
    public async Task WillNotOverwriteStatusCode_WhenHttpResultApplied()
    {
        var httpResult = new FakeHttpResult();
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(false, [httpResult]));
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Dispatch(It.IsAny<IMediatorAction>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediatorMock);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.True(httpResult.Applied);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WillApplyFirstHttpResult_WhenMultiplePresent()
    {
        var first = new FakeHttpResult();
        var second = new FakeHttpResult();
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, [first, second]));
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Dispatch(It.IsAny<IMediatorAction>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediatorMock);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var context = new FakeContext(new FakePostRequest(_message), services);
        await sut.Invoke(context);

        Assert.True(first.Applied);
        Assert.False(second.Applied);
    }

    private async Task ExecuteRequest(HttpRequest request)
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse<string>)new MediatorResponse<string>(true, Array.Empty<object>()));
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Execute<string>(It.IsAny<NopRequest>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediatorMock);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var context = new FakeContext(request, services);
        await sut.Invoke(context);

        mediatorMock.Verify(m => m.Execute(It.IsAny<NopRequest>(), It.IsAny<CancellationToken>()));
    }

    private async Task ExecuteMessage(HttpRequest request)
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, Array.Empty<object>()));
        var mediatorMock = new Mock<IMediator>();
        mediatorMock.Setup(x => x.Dispatch(It.IsAny<NopMessage>(), It.IsAny<CancellationToken>())).Returns(mediatorResponse);

        var services = CreateServiceProvider(mediatorMock);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var context = new FakeContext(request, services);
        await sut.Invoke(context);

        mediatorMock.Verify(m => m.Dispatch(It.IsAny<NopMessage>(), It.IsAny<CancellationToken>()));
    }

    private ServiceProvider CreateServiceProvider(Mock<IMediator> mediatorMock)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddMediatorServer()
            .AddActions([typeof(NopRequest), typeof(NopMessage)]);
        collection.AddScoped<MediatorMiddleware>();
        collection.AddScoped<RequestDelegate>(s => (c) => Task.CompletedTask);
        collection.AddSingleton<IMediator>(mediatorMock.Object);
        return collection.BuildServiceProvider();
    }
}