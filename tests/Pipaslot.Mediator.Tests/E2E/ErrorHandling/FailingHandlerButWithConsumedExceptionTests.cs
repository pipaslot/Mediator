using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.ErrorHandling;

/// <summary>
/// Ensure that failure is returned event if there is some middleware consuming all exception produced by handler
/// </summary>
public class FailingHandlerButWithConsumedExceptionTests
{
    [Fact]
    public async Task Execute_SuccessAsFalse()
    {
        var sut = CreateMediator(typeof(SingleHandler.RequestHandler));
        var result = await sut.Execute(new SingleHandler.Request(false));
        Assert.False(result.Success);
    }

    [Fact]
    public async Task ExecuteUnhandled_ThrowMediatorException()
    {
        var sut = CreateMediator(typeof(SingleHandler.RequestHandler));
        await Assert.ThrowsAsync<MediatorUnhandledErrorException>(async () =>
        {
            await sut.ExecuteUnhandled(new SingleHandler.Request(false));
        });
        // We do not care about the message here
    }

    [Fact]
    public async Task Dispatch_SuccessAsFalse()
    {
        var sut = CreateMediator(typeof(SingleHandler.MessageHandler));
        var result = await sut.Dispatch(new SingleHandler.Message(false));
        Assert.False(result.Success);
    }

    [Fact]
    public async Task DispatchUnhandled_ThrowMediatorException()
    {
        var sut = CreateMediator(typeof(SingleHandler.MessageHandler));
        await Assert.ThrowsAsync<MediatorUnhandledErrorException>(async () =>
        {
            await sut.DispatchUnhandled(new SingleHandler.Message(false));
        });
        // We do not care about the message here
    }

    private static IMediator CreateMediator(Type handlerType)
    {
        return Factory.CreateMediator(c => c
            .AddHandlers([handlerType])
            .Use<ExceptionConsumingMiddleware>());
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