using Pipaslot.Mediator.Tests.InvalidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E;

public class NoHandlerButResultWasReturned
{
    [Fact]
    public async Task Execute_Success()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RequestWithoutHandlerAttacheResultMilldeware>());
        var result = await sut.Execute(new RequestWithoutHandler());
        Assert.True(result.Success);
        Assert.Equal(typeof(RequestWithoutHandler.ResultDto), result.Result.GetType());
    }

    [Fact]
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