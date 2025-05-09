using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Middlewares;

public class MultiHandlerConcurrentExecutionTests
{
    public MultiHandlerConcurrentExecutionTests()
    {
        ConcurrentHandler.ExecutedCount = 0;
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
        var services = Factory.CreateServiceProviderWithHandlers<ConcurrentHandler.RequestHandler1>();
        var context = await RunRequest(services);
        Assert.Equal(1, ConcurrentHandler.ExecutedCount);
    }

    [Fact]
    public async Task RequestWithMultipleHandlers_Pass()
    {
        var services = Factory.CreateServiceProviderWithHandlers<ConcurrentHandler.RequestHandler1, ConcurrentHandler.RequestHandler2>();
        var context = await RunRequest(services);
        Assert.Equal(2, ConcurrentHandler.ExecutedCount);
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
        var services = Factory.CreateServiceProviderWithHandlers<ConcurrentHandler.MessageHandler1>();
        var context = await RunMessage(services);
        Assert.Equal(1, ConcurrentHandler.ExecutedCount);
    }


    [Fact]
    public async Task MessageWithMultipleHandlers_Pass()
    {
        var services = Factory.CreateServiceProviderWithHandlers<ConcurrentHandler.MessageHandler1, ConcurrentHandler.MessageHandler2>();
        var context = await RunMessage(services);
        Assert.Equal(2, ConcurrentHandler.ExecutedCount);
    }

    private async Task<MediatorContext> RunRequest(IServiceProvider services)
    {
        var action = new ConcurrentHandler.Request(true);
        return await Run(services, action);
    }

    private async Task<MediatorContext> RunMessage(IServiceProvider services)
    {
        var action = new ConcurrentHandler.Message(true);
        return await Run(services, action);
    }

    private async Task<MediatorContext> Run(IServiceProvider services, IMediatorAction action)
    {
        var sut = new HandlerExecutionMiddleware(new MediatorConfigurator(new ServiceCollection()));
        var context = services.CreateMediatorContext(action);
        var next = Factory.CreateMiddlewareDelegate();
        await sut.Invoke(context, next);
        return context;
    }
}