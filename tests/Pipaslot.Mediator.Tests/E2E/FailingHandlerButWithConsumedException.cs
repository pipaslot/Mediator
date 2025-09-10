using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests.E2E;

/// <summary>
/// Ensure that failure is returned event if there is some middleware consuming all exception produced by handler
/// </summary>
public class FailingHandlerButWithConsumedException
{
    [Test]
    public async Task Execute_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<ExceptionConsumingMiddleware>());
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.False(result.Success);
    }

    [Test]
    public async Task ExecuteUnhandled_ThrowMediatorException()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<ExceptionConsumingMiddleware>());
        await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(false));
        });
        // We do not care about the message here
    }

    [Test]
    public async Task Dispatch_SuccessAsFalse()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<ExceptionConsumingMiddleware>());
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        Assert.False(result.Success);
    }

    [Test]
    public async Task DispatchUnhandled_ThrowMediatorException()
    {
        var sut = Factory.CreateConfiguredMediator(c => c.Use<ExceptionConsumingMiddleware>());
        await Assert.ThrowsAsync<MediatorExecutionException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(false));
        });
        // We do not care about the message here
    }

    public class ExceptionConsumingMiddleware : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            try
            {
                await next(context);
            }
            catch
            {
                // Catch all silently
            }
        }
    }
}