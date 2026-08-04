using Pipaslot.Mediator.Tests.E2E.Fixtures;
using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ErrorHandling;

/// <summary>
/// This case simulates validator intercepting the processing and returning error message. The handler is not executed on purpose.
/// </summary>
public class NoHandlerAndErrorReturnedTests
{
    private const string Error = AddErrorAndEndMiddleware.Error;

    [Fact]
    public async Task Execute_SuccessAsFalse()
    {
        var sut = CreateMediator();
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.False(result.Success);
        Assert.Equal(Error, result.GetErrorMessage());
        Assert.Null(result.Result);
    }

    [Fact]
    public async Task ExecuteUnhandled_ThrowException()
    {
        var sut = CreateMediator();
        var action = new SingleHandler.Request(true);
        var ex = await Assert.ThrowsAsync<MediatorUnhandledErrorException>(async () =>
        {
            await sut.ExecuteUnhandled(action);
        });
        Assert.Contains(Error, ex.Message);
    }

    [Fact]
    public async Task Dispatch_SuccessAsFalse()
    {
        var sut = CreateMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.False(result.Success);
        Assert.Equal(Error, result.GetErrorMessage());
    }

    [Fact]
    public async Task DispatchUnhandled_ThrowException()
    {
        var sut = CreateMediator();
        var action = new SingleHandler.Message(true);
        var ex = await Assert.ThrowsAsync<MediatorUnhandledErrorException>(async () =>
        {
            await sut.DispatchUnhandled(action);
        });
        Assert.Contains(Error, ex.Message);
    }

    private IMediator CreateMediator()
    {
        return Factory.CreateMediator(c => c.Use<AddErrorAndEndMiddleware>());
    }
}