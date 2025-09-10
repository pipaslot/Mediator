using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests.E2E;

public class BasicPassing
{
    [Test]
    public async Task Execute_SuccessAsTrue()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(true));
        await Assert.That(result.Success).IsTrue();
    }

    [Test]
    public async Task Execute_EmptyErrorMessage()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(true));
        await Assert.That(result.GetErrorMessage()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Execute_ResultReturnsDataFromHandler()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(true));
        await Assert.That(result.Result).IsEqualTo(SingleHandler.Response.Instance);
    }

    [Test]
    public async Task ExecuteUnhandled_ReturnsResult()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.ExecuteUnhandled(new SingleHandler.Request(true));
        await Assert.That(result).IsEqualTo(SingleHandler.Response.Instance);
    }

    [Test]
    public async Task Dispatch_SuccessAsTrue()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        await Assert.That(result.Success).IsTrue();
    }

    [Test]
    public async Task Dispatch_EmptyErrorMessage()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(true));
        await Assert.That(result.GetErrorMessage()).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task DispatchUnhandled_NoAction()
    {
        var sut = Factory.CreateConfiguredMediator();
        await sut.DispatchUnhandled(new SingleHandler.Message(true));
    }
}