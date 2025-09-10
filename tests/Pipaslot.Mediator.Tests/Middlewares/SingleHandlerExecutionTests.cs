using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;

namespace Pipaslot.Mediator.Tests.Middlewares;

public class SingleHandlerExecutionTests
{
    public SingleHandlerExecutionTests()
    {
        SingleHandler.ExecutedCount = 0;
    }

    [Test]
    public async Task RequestWithSingleHandler_ExecuteHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.RequestHandler>();
        await RunRequest(services);
        Assert.Equal(1, SingleHandler.ExecutedCount);
    }

    [Test]
    public async Task RequestWithoutHandler_DoNotThrowException()
    {
        var services = Factory.CreateServiceProvider();
        await RunRequest(services);
    }

    [Test]
    public async Task RequestWithMultipleHandlers_ThrowException()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.RequestHandler, SingleHandler.RequestHandler>();
        await Assert.ThrowsAsync<MediatorException>(async () =>
        {
            await RunRequest(services);
        });
    }

    [Test]
    public async Task MessageWithSingleHandler_ExecuteHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.MessageHandler>();
        var context = await RunMessage(services);
        Assert.Equal(1, SingleHandler.ExecutedCount);
    }

    [Test]
    public async Task MessageWithoutHandler_DoNotThrowException()
    {
        var services = Factory.CreateServiceProvider();
        await RunMessage(services);
    }

    [Test]
    public async Task MessageWithMultipleHandlers_ThrowException()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SingleHandler.MessageHandler, SingleHandler.MessageHandler>();
        await Assert.ThrowsAsync<MediatorException>(async () =>
        {
            await RunMessage(services);
        });
    }

    private async Task<MediatorContext> RunRequest(IServiceProvider services)
    {
        var action = new SingleHandler.Request(true);
        return await Run(services, action);
    }

    private async Task<MediatorContext> RunMessage(IServiceProvider services)
    {
        var action = new SingleHandler.Message(true);
        return await Run(services, action);
    }

    private async Task<MediatorContext> Run(IServiceProvider services, IMediatorAction action)
    {
        var context = services.CreateMediatorContext(action);
        var sut = new HandlerExecutionMiddleware();
        var next = Factory.CreateMiddlewareDelegate();
        await sut.Invoke(context, next);
        return context;
    }
}