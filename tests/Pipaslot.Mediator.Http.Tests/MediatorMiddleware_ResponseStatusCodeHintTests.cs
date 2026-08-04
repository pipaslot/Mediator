using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Tests.Fakes;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Tests;

/// <summary>
/// <see cref="MediatorMiddleware"/> coverage for <see cref="ResponseStatusCodeHint"/> specifically: applying it to
/// the HTTP response, precedence against a status code already set by legacy middleware or by
/// <see cref="IMediatorHttpResult"/>, last-one-wins when several are present, being skipped once the response has
/// already started, and being excluded from the serialized response body. Split out of
/// <c>MediatorMiddlewareTests</c> because this is a single, named production concept rather than one more
/// general-purpose Dispatch/Execute propagation case.
/// </summary>
public class MediatorMiddleware_ResponseStatusCodeHintTests
{
    private const string _message =
        "{\"$type\":\"Pipaslot.Mediator.Http.Tests.MediatorMiddleware_ResponseStatusCodeHintTests+NopMessage, Pipaslot.Mediator.Http.Tests\"}";

    [Fact]
    public async Task WillApplyStatusCodeHint_WhenPresent()
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, [new ResponseStatusCodeHint(400)]));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task WillOverwriteAlreadySetStatusCode_WhenHintPresent()
    {
        // A hint always wins over whatever a legacy middleware already wrote directly to HttpContext.Response.
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(false, [new ResponseStatusCodeHint(409)]));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse { StatusCode = (int)HttpStatusCode.BadRequest };
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.Equal(409, response.StatusCode);
    }

    [Fact]
    public async Task WillUseLastStatusCodeHint_WhenMultiplePresent()
    {
        // A later hint overrides an earlier one, mirroring what MediatorContext.Features.Set() would do.
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true,
            [new ResponseStatusCodeHint(400), new ResponseStatusCodeHint(409)]));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.Equal(409, response.StatusCode);
    }

    [Fact]
    public async Task WillNotApplyStatusCodeHint_WhenResponseAlreadyStarted()
    {
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, [new ResponseStatusCodeHint(400)]));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);
        var services = CreateServiceProvider(mediator);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse(hasStarted: true);
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        // Default JSON serialization must also be skipped once something else already wrote to the response
        Assert.Equal(string.Empty, response.ContentType);
    }

    [Fact]
    public async Task WillIgnoreStatusCodeHint_WhenHttpResultPresent()
    {
        // IMediatorHttpResult owns the whole response and takes precedence over a mere status-code annotation.
        var httpResult = new FakeHttpResult();
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, [httpResult, new ResponseStatusCodeHint(400)]));
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
    public async Task WillExcludeStatusCodeHint_FromSerializedResponseBody()
    {
        const string otherResult = "actual-result";
        var mediatorResponse = Task.FromResult((IMediatorResponse)new MediatorResponse(true, [otherResult, new ResponseStatusCodeHint(400)]));
        var mediator = Substitute.For<IMediator>();
        mediator.Dispatch(Arg.Any<IMediatorAction>(), Arg.Any<CancellationToken>()).Returns(mediatorResponse);

        IMediatorResponse? serialized = null;
        var serializer = Substitute.For<IContractSerializer>();
        serializer.SerializeResponse(Arg.Do<IMediatorResponse>(r => serialized = r)).Returns("{}");

        var services = CreateServiceProvider(mediator, serializer);
        var sut = services.GetRequiredService<MediatorMiddleware>();

        var response = new FakeResponse();
        var context = new FakeContext(new FakePostRequest(_message), services, response);
        await sut.Invoke(context);

        Assert.NotNull(serialized);
        Assert.DoesNotContain(serialized!.Results, r => r is ResponseStatusCodeHint);
        Assert.Contains(otherResult, serialized.Results);
    }

    private ServiceProvider CreateServiceProvider(IMediator mediator, IContractSerializer? serializer = null)
    {
        var collection = new ServiceCollection();
        collection.AddLogging();
        collection.AddMediatorServer()
            .AddActions([typeof(NopMessage)]);
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
}
