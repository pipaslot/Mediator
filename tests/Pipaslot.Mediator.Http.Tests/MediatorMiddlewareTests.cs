using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Tests.Fakes;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests;

public class MediatorMiddlewareTests
{
    private const string _request =
        "{\"$type\":\"Pipaslot.Mediator.Http.Tests.MediatorMiddlewareTests+NopRequest, Pipaslot.Mediator.Http.Tests\"}";

    private const string _message =
        "{\"$type\":\"Pipaslot.Mediator.Http.Tests.MediatorMiddlewareTests+NopMessage, Pipaslot.Mediator.Http.Tests\"}";

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
        // No filter ever registered - GET stays unrestricted, unchanged from previous versions. (S2)
        await ExecuteMessage(new FakeGetRequest(_message));
    }

    [Fact]
    public async Task GetRequestWillBePropagatedToMediator()
    {
        // (S2)
        await ExecuteRequest(new FakeGetRequest(_request));
    }

    [Fact]
    public async Task GetMessageWillBePropagatedToMediator_WhenFilterAllowsIt()
    {
        // (S1)
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, Array.Empty<object>()));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<NopMessage>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator, configure: o => o.HttpGetConditions.Allow(a => a is NopMessage));
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var context = new FakeContext(new FakeGetRequest(_message), services);
        await sut.Invoke(context);

        await mediator.Received().Dispatch(Arg.Any<NopMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMessageWillNotBePropagatedToMediator_WhenNoFilterAllowsIt()
    {
        // (S3) The rejection error text must also reference the current predicate-based API rather than the
        // removed type-based method names. (S7)
        var mediator = Substitute.For<IMediator>();

        IMediatorResponse? serialized = null;
        var serializer = Substitute.For<IContractSerializer>();
        serializer.DeserializeRequest(Arg.Any<string>(), Arg.Any<ICollection<StreamContract>>())
            .Returns(new NopMessage());
        serializer.SerializeResponse(Arg.Do<IMediatorResponse>(r => serialized = r))
            .Returns("{}");

        var services = CreateServiceProvider(mediator, serializer, configure: o => o.HttpGetConditions.Allow(_ => false));
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakeGetRequest(_message), services, response);
        await sut.Invoke(context);

        await mediator.DidNotReceive().Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>());
        Assert.Equal(MediatorConstants.ErrorHttpStatusCode, response.StatusCode);

        var errorMessage = Assert.Single(serialized!.Results.Cast<Notification>()).Content;
        Assert.Contains(nameof(ServerMediatorOptions.HttpGetConditions), errorMessage);
        Assert.Contains(nameof(HttpGetActionAllowlist.Allow), errorMessage);
        Assert.DoesNotContain("AddAllowedHttpGetActionType", errorMessage);
    }

    [Fact]
    public async Task PostMessageWillBePropagatedToMediator_WhenNoFilterWouldAllowItOverGet()
    {
        // The allowlist only applies to HTTP GET, POST must remain unaffected regardless of it. (S4)
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, Array.Empty<object>()));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<NopMessage>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator, configure: o => o.HttpGetConditions.Allow(_ => false));
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var context = new FakeContext(new FakePostRequest(_message), services);
        await sut.Invoke(context);

        await mediator.Received().Dispatch(Arg.Any<NopMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task WillNotWriteResponse_WhenResponseAlreadyHasStarted()
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, Array.Empty<object>()));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
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
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
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
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
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
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
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
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.True(httpResult.Applied);
        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task WillThrow_WhenMultipleHttpResultsPresent()
    {
        var first = new FakeHttpResult();
        var second = new FakeHttpResult();
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, [first, second]));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var context = new FakeContext(new FakePostRequest(_message), services);

        var ex = await Assert.ThrowsAsync<MediatorHttpException>(() => sut.Invoke(context));

        Assert.Contains("2", ex.Message);
        Assert.False(first.Applied);
        Assert.False(second.Applied);
    }

    /// <summary>
    /// Integration-level sanity check for safe-by-default error handling across the real HTTP pipeline: a real
    /// handler throws, the dispatch boundary converts it to a generic failure, and the JSON serializer writes the
    /// response body. This verifies that internal exception details never leak to the wire payload, not just to the
    /// in-memory <see cref="IMediatorResponse"/> passed between components.
    /// </summary>
    [Fact]
    public async Task UnmappedExceptionFromServerHandler_NeverPutsInternalDetailOnTheWire()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatorServer()
            .AddActions([typeof(ThrowingMessage)])
            .AddHandlers([typeof(ThrowingMessageHandler)]);
        services.AddScoped<MediatorMiddleware>();
        services.AddScoped<RequestDelegate>(s => (c) => Task.CompletedTask);
        var provider = services.BuildServiceProvider();

        var serializer = provider.GetRequiredService<IContractSerializer>();
        var body = serializer.SerializeRequest(new ThrowingMessage()).Json;

        var sut = provider.GetRequiredService<MediatorMiddleware>();
        var response = new FakeResponse { Body = new MemoryStream() };
        var context = new FakeContext(new FakePostRequest(body), provider, response);

        await sut.Invoke(context);

        response.Body.Position = 0;
        var wireContent = await new StreamReader(response.Body).ReadToEndAsync();

        Assert.DoesNotContain(ThrowingMessageHandler.InternalDetail, wireContent);
        Assert.Contains(Mediator.GenericErrorMessage, wireContent);
    }

    public class ThrowingMessage : IMessage;

    public class ThrowingMessageHandler : IMessageHandler<ThrowingMessage>
    {
        public const string InternalDetail = "connection string secret detail";

        public Task Handle(ThrowingMessage action, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(InternalDetail);
        }
    }

    private async Task ExecuteRequest(HttpRequest request)
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse<string>)new MediatorResponse<string>(true, Array.Empty<object>()));
        var mediator = Substitute.For<IMediator>();
        mediator.Execute<string>(Arg.Any<NopRequest>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var context = new FakeContext(request, services);
        await sut.Invoke(context);

        await mediator.Received().Execute(Arg.Any<NopRequest>(), Arg.Any<CancellationToken>());
    }

    private async Task ExecuteMessage(HttpRequest request)
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, Array.Empty<object>()));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<NopMessage>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);

        var services = CreateServiceProvider(mediator);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var context = new FakeContext(request, services);
        await sut.Invoke(context);

        await mediator.Received().Dispatch(Arg.Any<NopMessage>(), Arg.Any<CancellationToken>());
    }

    private ServiceProvider CreateServiceProvider(IMediator mediator, IContractSerializer? serializer = null,
        Action<ServerMediatorOptions>? configure = null)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddMediatorServer(configure ?? (_ => { }))
            .AddActions([typeof(NopRequest), typeof(NopMessage)]);
        collection.AddScoped<MediatorMiddleware>();
        collection.AddScoped<RequestDelegate>(s => (c) => Task.CompletedTask);
        collection.AddSingleton(mediator);
        if (serializer is not null)
        {
            collection.AddSingleton(serializer);
        }
        return collection.BuildServiceProvider();
    }
    public class NopMessage : IMessage;

    public class NopRequest : IRequest<string>;
}
