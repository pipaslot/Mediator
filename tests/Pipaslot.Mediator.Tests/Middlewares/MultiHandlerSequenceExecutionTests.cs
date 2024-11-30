using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

public class MultiHandlerSequenceExecutionTests
{
    public MultiHandlerSequenceExecutionTests()
    {
        SequenceHandler.ExecutedCount = 0;
    }

    [Fact]
    public async Task RequestWithoutHandler_DoNotThrowException()
    {
        var services = Factory.CreateServiceProvider();
        await RunRequest(services);
    }

    [Fact]
    public async Task RequestWithSingleHandler_ExecuteHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.RequestHandler1>();
        var context = await RunRequest(services);
        Assert.Equal(1, SequenceHandler.ExecutedCount);
    }

    [Fact]
    public async Task RequestWithMultipleHandlers_ThrowException()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.RequestHandler1, SequenceHandler.RequestHandler2>();
        var context = await RunRequest(services);
        Assert.Equal(2, SequenceHandler.ExecutedCount);
    }

    [Fact]
    public async Task MessageWithoutHandler_DoNotThrowException()
    {
        var services = Factory.CreateServiceProvider();
        await RunMessage(services);
    }

    [Fact]
    public async Task MessageWithSingleHandler_ExecuteHandler()
    {
        var services = Factory.CreateServiceProviderWithHandlers<SequenceHandler.MessageHandler1>();
        var context = await RunMessage(services);
        Assert.Equal(1, SequenceHandler.ExecutedCount);
    }


    [Fact]
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
        var mediator = new Mock<IMediator>();
        var sut = new HandlerExecutionMiddleware();
        var mcaMock = new Mock<IMediatorContextAccessor>();
        var context = new MediatorContext(mediator.Object, mcaMock.Object, services, action, CancellationToken.None, null, null);
        var next = Factory.CreateMiddlewareDelegate();
        await sut.Invoke(context, next);
        return context;
    }
}