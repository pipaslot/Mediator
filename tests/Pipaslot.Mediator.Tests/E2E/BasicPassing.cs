using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E;

public class BasicPassing
{
    [Fact]
    public async Task Execute_SuccessAsTrue()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Execute_EmptyErrorMessage()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.Equal(string.Empty, result.GetErrorMessage());
    }

    [Fact]
    public async Task Execute_ResultReturnsDataFromHandler()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(true));
        Assert.Equal(SingleHandler.Response.Instance, result.Result);
    }

    [Fact]
    public async Task ExecuteUnhandled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        Assert.Equal(SingleHandler.Response.Instance, result);
    }

    [Fact]
    public async Task Dispatch_SuccessAsTrue()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.True(result.Success);
    }

    [Fact]
    public async Task Dispatch_EmptyErrorMessage()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        Assert.Equal(string.Empty, result.GetErrorMessage());
    }

    [Fact]
    public async Task DispatchUnhandled_NoAction()
    {
        var sut = Factory.CreateConfiguredMediator();
        await sut.DispatchUnhandled(new SingleHandler.Message(true));
    }
}