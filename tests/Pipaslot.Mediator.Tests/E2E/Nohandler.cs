using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Tests.InvalidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E;

public class Nohandler
{
    [Fact]
    public async Task Execute_FailWithGenericErrorBecauseNoHandlerIsConfigured()
    {
        var sut = Factory.CreateConfiguredMediator();
        var action = new RequestWithoutHandler();
        var result = await sut.Execute(action);
        
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_LogsOriginalExceptionDetailAtErrorLevel()
    {
        var (sut, logger) = Factory.CreateConfiguredMediatorWithLogger();

        await sut.Execute(new RequestWithoutHandler());

        var entry = Assert.Single(logger.Entries, e => e.Level == LogLevel.Error);
        Assert.IsType<MediatorNoHandlerFoundException>(entry.Exception);
    }

    [Fact]
    public async Task ExecuteUnhandled_ThrowMissingResultException()
    {
        var sut = Factory.CreateConfiguredMediator();
        var action = new RequestWithoutHandler();
        var ex =
            await Assert.ThrowsAsync<MediatorNoHandlerFoundException>(async () =>
            {
                await sut.ExecuteUnhandled(action);
            });
        Assert.Equal(MediatorNoHandlerFoundException.Create(action.GetType()).Message, ex.Message);
    }

    [Fact]
    public async Task Dispatch_ReturnFailureBecauseNotHandlerWasExecuted()
    {
        var sut = Factory.CreateConfiguredMediator();
        var action = new MessageWithoutHandler();
        var result = await sut.Dispatch(action);
        Assert.False(result.Success);
        Assert.Equal(Mediator.GenericErrorMessage, result.GetErrorMessage());
    }

    [Fact]
    public async Task DispatchUnhandled_ThrowNoHandlerException()
    {
        var sut = Factory.CreateConfiguredMediator();
        var action = new RequestWithoutHandler();
        var ex =
            await Assert.ThrowsAsync<MediatorNoHandlerFoundException>(async () =>
            {
                await sut.DispatchUnhandled(action);
            });
        Assert.Equal(MediatorNoHandlerFoundException.Create(action.GetType()).Message, ex.Message);
    }
}