using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E;

/// <summary>
/// No result is returned and Context status is set to Failed. No error message was added so no error should be produced
/// </summary>
public class NoHandlerWithoutError
{
    [Fact]
    public async Task Execute_FailedWithoutError()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<BlockRequestMilldeware>());
        var action = new BlockedRequest();
        var result = await sut.Execute(action);
        Assert.False(result.Success);
        Assert.Empty(result.Results);
        Assert.Equal(string.Empty, result.GetErrorMessage());
    }

    [Fact]
    public async Task ExecuteUnhandled_FailedWithoutError()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<BlockRequestMilldeware>());
        var action = new BlockedRequest();
        var ex =
            await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.ExecuteUnhandled(action);
            });
        var context = Factory.FakeContext(action);
        Assert.Equal(MediatorExecutionException.CreateForUnhandledError(context).Message, ex.Message);
    }

    [Fact]
    public async Task Dispatch_FailedWithoutError()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<BlockRequestMilldeware>());
        var action = new BlockedRequest();
        var result = await sut.Dispatch(action);
        Assert.False(result.Success);
        Assert.Empty(result.Results);
        Assert.Equal(string.Empty, result.GetErrorMessage());
    }

    [Fact]
    public async Task DispatchUnhandled_Exception()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<BlockRequestMilldeware>());
        var action = new BlockedRequest();
        var ex =
            await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
            {
                await sut.DispatchUnhandled(action);
            });
        var context = Factory.FakeContext(action);
        Assert.Equal(MediatorExecutionException.CreateForUnhandledError(context).Message, ex.Message);
    }

    public class BlockedRequest : IRequest<BlockedRequest.ResultDto>
    {
        public class ResultDto
        {
        }
    }

    public class BlockedMessage : IMessage
    {
    }

    public class BlockRequestMilldeware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.Status = ExecutionStatus.Failed;
            // Do not run next delegate
            return Task.CompletedTask;
        }
    }
}