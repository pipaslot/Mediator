using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.Outcomes;

public class BasicFailingTests
{
    [Fact]
    public async Task Execute_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Execute_GenericErrorDueToMissingExceptionHandler()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_NullResult()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task Execute_LogsOriginalExceptionDetailAtErrorLevel()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger();

        await sut.Execute(new SingleHandler.Request(false));

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.IsType<SingleHandler.RequestException>(entry.Exception);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task ExecuteUnhandled_ThrowOriginalException()
    {
        var sut = Factory.CreateConfiguredMediator();
        var ex = await Assert.ThrowsAsync<SingleHandler.RequestException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(false));
        });
        Assert.Equal(SingleHandler.RequestException.DefaultMessage, ex.Message);
    }

    [Fact]
    public async Task Dispatch_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Dispatch_GenericErrorDueToMissingExceptionHandler()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Dispatch_LogsOriginalExceptionDetailAtErrorLevel()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger();

        await sut.Dispatch(new SingleHandler.Message(false));

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.IsType<SingleHandler.MessageException>(entry.Exception);
        Assert.DoesNotContain(logger.Entries, e => e.Level == LogLevel.Warning);
    }

    [Fact]
    public async Task DispatchUnhandled_ThrowOriginalException()
    {
        var sut = Factory.CreateConfiguredMediator();
        var ex = await Assert.ThrowsAsync<SingleHandler.MessageException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(false));
        });
        Assert.Equal(SingleHandler.MessageException.DefaultMessage, ex.Message);
    }
}