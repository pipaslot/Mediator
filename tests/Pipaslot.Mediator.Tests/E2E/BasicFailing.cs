using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests.E2E;

public class BasicFailing
{
    [Test]
    public async Task Execute_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task Execute_NotEmptyErrorMessage()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        await Assert.That(result.GetErrorMessage()).IsEqualTo(SingleHandler.RequestException.DefaultMessage);
    }

    [Test]
    public async Task Execute_NullResult()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Execute(new SingleHandler.Request(false));
        await Assert.That(result.Result).IsNull();
    }

    [Test]
    public async Task ExecuteUnhandled_ThrowOriginalException()
    {
        var sut = Factory.CreateConfiguredMediator();
        await Assert.That(async () => await sut.ExecuteUnhandled(new SingleHandler.Request(false)))
            .Throws<SingleHandler.RequestException>();
        // We do not care about the error message as we only expect the original exception
    }

    [Test]
    public async Task Dispatch_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        await Assert.That(result.Success).IsFalse();
    }

    [Test]
    public async Task Dispatch_NotEmptyErrorMessage()
    {
        var sut = Factory.CreateConfiguredMediator();
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        await Assert.That(result.GetErrorMessage()).IsEqualTo(SingleHandler.MessageException.DefaultMessage);
    }

    [Test]
    public async Task DispatchUnhandled_ThrowOriginalException()
    {
        var sut = Factory.CreateConfiguredMediator();
        await Assert.That(async () => await sut.DispatchUnhandled(new SingleHandler.Message(false)))
            .Throws<SingleHandler.MessageException>();
        // We do not care about the error message as we only expect the original exception
    }
}