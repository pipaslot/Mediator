using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests;

public class MediatorExceptionLoggingMiddlewareTests
{
    [Fact]
    public async Task Execute_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsSuccessFalse()
    {
        var sut = CreateMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Execute_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsErrorMessage()
    {
        var sut = CreateMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.Equal(SingleHandler.RequestException.DefaultMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task ExecuteUnhandled_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsException()
    {
        var sut = CreateMediator();
        await Assert.ThrowsAsync<SingleHandler.RequestException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(false));
        });
    }

    [Fact]
    public async Task Dispatch_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsSuccessFalse()
    {
        var sut = CreateMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Dispatch_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsErrorMessage()
    {
        var sut = CreateMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        Assert.Equal(SingleHandler.MessageException.DefaultMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task DispatchUnhandled_ExceptionCatchedByMiddlewareIsPropagatedOutOfMediatorAsException()
    {
        var sut = CreateMediator();
        await Assert.ThrowsAsync<SingleHandler.MessageException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(false));
        });
    }

    private static IMediator CreateMediator()
    {
        return Factory.CreateMediator(c => c.UseExceptionLogging());
    }
}