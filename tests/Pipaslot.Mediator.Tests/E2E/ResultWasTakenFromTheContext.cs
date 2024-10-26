using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.InvalidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E;

public class ResultWasTakenFromTheContext
{
    [Fact]
    public async Task Execute_ReturnFailureBecauseNotResultWasFound()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RemoveResultFromContextMilldeware>());
        var action = new RequestWithoutHandler();
        var result = await sut.Execute(action);
        Assert.False(result.Success);
        var context = Factory.FakeContext(action);
        Assert.Equal(MediatorExecutionException.CreateForMissingResult(context, typeof(RequestWithoutHandler.ResultDto)).Message,
            result.GetErrorMessage());
    }

    [Fact]
    public async Task ExecuteUnhandled_ThrowMissingResultException()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<RemoveResultFromContextMilldeware>());
        var action = new RequestWithoutHandler();
        var ex =
            await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.ExecuteUnhandled(action);
            });
        var context = Factory.FakeContext(action);
        Assert.Equal(MediatorExecutionException.CreateForMissingResult(context, typeof(RequestWithoutHandler.ResultDto)).Message, ex.Message);
    }

    // Does not make sense for Dispatch and DispatchUnhandled

    public class RemoveResultFromContextMilldeware : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            // The result will be attached to the fake context instead to the actual one
            var fakeContext = context.CopyEmpty();
            await next(fakeContext);
        }
    }
}