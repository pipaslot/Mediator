using Pipaslot.Mediator.Tests.E2E.Fixtures;
using Pipaslot.Mediator.Tests.InvalidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ErrorHandling;

public class NoHandlerButResultWasReturnedTests
{
    [Fact]
    public async Task Execute_Success()
    {
        var sut = CreateMediator();
        var result = await sut.Execute(new RequestWithoutHandler());
        Assert.True(result.Success);
        Assert.Equal(typeof(RequestWithoutHandler.ResultDto), result.Result.GetType());
    }

    [Fact]
    public async Task ExecuteUnhandled_Success()
    {
        var sut = CreateMediator();
        var action = new RequestWithoutHandler();
        var dto = await sut.ExecuteUnhandled(action);
        Assert.Equal(typeof(RequestWithoutHandler.ResultDto), dto.GetType());
    }

    // Not relevant for Dispatch and DispatchUnhandled

    private IMediator CreateMediator()
    {
        return Factory.CreateMediator(c => c.Use<RequestWithoutHandlerAttacheResultMiddleware>());
    }
}