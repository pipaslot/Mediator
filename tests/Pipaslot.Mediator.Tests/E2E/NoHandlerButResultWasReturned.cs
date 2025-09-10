using Pipaslot.Mediator.Tests.InvalidActions;

namespace Pipaslot.Mediator.Tests.E2E;

public class NoHandlerButResultWasReturned
{
    [Test]
    public async Task Execute_Success()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RequestWithoutHandlerAttacheResultMilldeware>());
        var result = await sut.Execute(new RequestWithoutHandler());
        Assert.True(result.Success);
        Assert.Equal(typeof(RequestWithoutHandler.ResultDto), result.Result.GetType());
    }

    [Test]
    public async Task ExecuteUnhandled_Success()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RequestWithoutHandlerAttacheResultMilldeware>());
        var action = new RequestWithoutHandler();
        var dto = await sut.ExecuteUnhandled(action);
        var context = Factory.FakeContext(action);
        Assert.Equal(typeof(RequestWithoutHandler.ResultDto), dto.GetType());
    }

    // Not relevant for Dispatch and DispatchUnhandled
}