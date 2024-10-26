using Pipaslot.Mediator.Tests.InvalidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E;

public class Nohandler
{
    [Fact]
    public async Task Execute_ReturnFailureBecauseNotResultWasFound()
    {
        var sut = Factory.CreateConfiguredMediator();
        var action = new RequestWithoutHandler();
        var result = await sut.Execute(action);
        Assert.False(result.Success);
        var context = Factory.FakeContext(action);
        Assert.Equal(MediatorExecutionException.CreateForNoHandler(action.GetType()).Message, result.GetErrorMessage());
    }

    [Fact]
    public async Task ExecuteUnhandled_ThrowMissingResultException()
    {
        var sut = Factory.CreateConfiguredMediator();
        var action = new RequestWithoutHandler();
        var ex =
            await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.ExecuteUnhandled(action);
            });
        var context = Factory.FakeContext(action);
        Assert.Equal(MediatorExecutionException.CreateForNoHandler(action.GetType()).Message, ex.Message);
    }

    [Fact]
    public async Task Dispatch_ReturnFailureBecauseNotHandlerWasExecuted()
    {
        var sut = Factory.CreateConfiguredMediator();
        var action = new MessageWithoutHandler();
        var result = await sut.Dispatch(action);
        Assert.False(result.Success);
        Assert.Equal(MediatorExecutionException.CreateForNoHandler(action.GetType()).Message, result.GetErrorMessage());
    }

    [Fact]
    public async Task DispatchUnhandled_ThrowNoHandlerException()
    {
        var sut = Factory.CreateConfiguredMediator();
        var action = new RequestWithoutHandler();
        var ex =
            await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.DispatchUnhandled(action);
            });
        var context = Factory.FakeContext(action);
        Assert.Equal(MediatorExecutionException.CreateForNoHandler(action.GetType()).Message, ex.Message);
    }
}