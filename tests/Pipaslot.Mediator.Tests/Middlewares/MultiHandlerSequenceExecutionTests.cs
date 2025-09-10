using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;

namespace Pipaslot.Mediator.Tests.Middlewares;

public class MultiHandlerSequenceExecutionTests
{
    public MultiHandlerSequenceExecutionTests()
    {
        SequenceHandler.ExecutedCount = 0;
    }

    [Test]
    public async Task RequestWithoutHandler_DoNotThrowException()
    {
        var services = Factory.CreateServiceProvider();
        await RunRequest(services);
    }

    [Test]
    public async Task RequestWithSingleHandler_ExecuteHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.RequestHandler1>();
        var context = await RunRequest(services);
        Assert.Equal(1, SequenceHandler.ExecutedCount);
    }

    [Test]
    public async Task RequestWithMultipleHandlers_ThrowException()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.RequestHandler1, SequenceHandler.RequestHandler2>();
        var context = await RunRequest(services);
        Assert.Equal(2, SequenceHandler.ExecutedCount);
    }

    [Test]
    public async Task MessageWithoutHandler_DoNotThrowException()
    {
        var services = Factory.CreateServiceProvider();
        await RunMessage(services);
    }

    [Test]
    public async Task MessageWithSingleHandler_ExecuteHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.MessageHandler1>();
        var context = await RunMessage(services);
        Assert.Equal(1, SequenceHandler.ExecutedCount);
    }


    [Test]
    public async Task MessageWithMultipleHandlers_ThrowException()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.MessageHandler1, SequenceHandler.MessageHandler2>();
        var context = await RunMessage(services);
        Assert.Equal(2, SequenceHandler.ExecutedCount);
    }

    private async Task<MediatorContext> RunRequest(IServiceProvider services)
    {
        var action = new SequenceHandler.Request(true);
        return await Run(services, action);
    }

    private async Task<MediatorContext> RunMessage(IServiceProvider services)
    {
        var action = new SequenceHandler.Message(true);
        return await Run(services, action);
    }

    private async Task<MediatorContext> Run(IServiceProvider services, IMediatorAction action)
    {
        var sut = new HandlerExecutionMiddleware();
        var context = services.CreateMediatorContext(action);
        var next = Factory.CreateMiddlewareDelegate();
        await sut.Invoke(context, next);
        return context;
    }
}