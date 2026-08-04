using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Services;
using Pipaslot.Mediator.Tests.InvalidActions;
using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests.Services;

public class HandlerExistenceCheckerTests
{
    [Fact]
    public void Verify_RegisteredAssemblyWithValidActions_DoesNotThrowExceptions()
    {
        // ValidActions/ shares this project's assembly with every other test fixture, so scanning the whole
        // assembly here would also sweep in handlers meant for manual DI wiring elsewhere (e.g.
        // ActionEventsMiddlewareTests.SemaphoreHandler) and fail to construct them. List the known-valid
        // types explicitly instead of an assembly-wide scan.
        var actionTypes = new[]
        {
            typeof(NopMessage), typeof(NopRequest),
            typeof(SingleHandler.Message), typeof(SingleHandler.Request),
            typeof(ConcurrentHandler.Message), typeof(ConcurrentHandler.Request),
            typeof(SequenceHandler.Message), typeof(SequenceHandler.Request),
            typeof(ScopedMessage), typeof(SingletonMessage), typeof(InstanceCounterMessage)
        };
        var handlerTypes = new[]
        {
            typeof(NopMesageHandler), typeof(NopRequestHandler),
            typeof(SingleHandler.MessageHandler), typeof(SingleHandler.RequestHandler),
            typeof(ConcurrentHandler.MessageHandler1), typeof(ConcurrentHandler.MessageHandler2),
            typeof(ConcurrentHandler.RequestHandler1), typeof(ConcurrentHandler.RequestHandler2),
            typeof(SequenceHandler.MessageHandler1), typeof(SequenceHandler.MessageHandler2),
            typeof(SequenceHandler.RequestHandler1), typeof(SequenceHandler.RequestHandler2),
            typeof(ScopedMessageHandler), typeof(SingletonMessageHandler), typeof(InstanceCounterMessageHandler)
        };
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddActions(actionTypes);
            c.AddHandlers(handlerTypes);
        });
        var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
        sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
    }

    [Fact]
    public void Verify_MessageWithoutHandler_ThrowExceptions()
    {
        ShouldThrow(MediatorNoHandlerFoundException.Create(typeof(MessageWithoutHandler)).Message);
    }

    [Fact]
    public void Verify_RequestWithoutHandler_ThrowExceptions()
    {
        ShouldThrow(MediatorNoHandlerFoundException.Create(typeof(RequestWithoutHandler)).Message);
    }

    private void ShouldThrow(string expectedError)
    {
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddActionsFromAssemblyOf<MessageWithoutHandler>();
            c.AddHandlersFromAssemblyOf<MessageWithoutHandler>();
        });
        var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
        var ex = Assert.Throws<MediatorException>(() =>
        {
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        });

        Assert.Contains(expectedError, ex.Message);
    }

    [Fact]
    public void Verify_ActionsWithoutHandlerThrowException()
    {
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddActions([typeof(InvalidActionWithoutHandler)]);
        });
        var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
        var ex = Assert.Throws<MediatorException>(() =>
        {
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        });
        var actualMessage = ex.Data["Error:1"]?.ToString() ?? string.Empty;
        Assert.Contains(nameof(InvalidActionWithoutHandler), actualMessage);
    }

    [Fact]
    public void Verify_ActionsWithoutHandlerButWithNoHandlerAttribute_Pass()
    {
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddActions([typeof(ValidActionWithoutHandler)]);
        });
        var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
        sut.Verify(new ExistenceCheckerSetting());
    }
}