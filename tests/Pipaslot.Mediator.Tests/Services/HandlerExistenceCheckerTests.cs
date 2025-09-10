using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Services;
using Pipaslot.Mediator.Tests.InvalidActions;
using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests.Services;

public class HandlerExistenceCheckerTests
{
    [Test]
    public async Task Verify_RegisteredAssemblyWithValidActions_DoesNotThrowExceptions()
    {
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddActionsFromAssemblyOf<NopMessage>();
            c.AddHandlersFromAssemblyOf<NopMesageHandler>();
        });
        var sut = sp.GetRequiredService<IHandlerExistenceChecker>();
        sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        await Task.CompletedTask;
    }

    [Test]
    public async Task Verify_MessageWithoutHandler_ThrowExceptions()
    {
        await ShouldThrow(MediatorExecutionException.CreateForNoHandler(typeof(MessageWithoutHandler)).Message);
    }

    [Test]
    public async Task Verify_RequestWithoutHandler_ThrowExceptions()
    {
        await ShouldThrow(MediatorExecutionException.CreateForNoHandler(typeof(RequestWithoutHandler)).Message);
    }

    private async Task ShouldThrow(string expectedError)
    {
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddActionsFromAssemblyOf<MessageWithoutHandler>();
            c.AddHandlersFromAssemblyOf<MessageWithoutHandler>();
        });
        var sut = sp.GetRequiredService<IHandlerExistenceChecker>();

        await Assert.That(() =>
        {
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        }).Throws<MediatorException>().And.HasMessageContaining(expectedError);
    }

    [Test]
    public async Task Verify_ActionsWithoutHandlerThrowException()
    {
        var sp = Factory.CreateServiceProvider(c =>
        {
            c.AddActions([typeof(InvalidActionWithoutHandler)]);
        });
        var sut = sp.GetRequiredService<IHandlerExistenceChecker>();

        var ex = await Assert.That(() =>
        {
            sut.Verify(new ExistenceCheckerSetting { CheckMatchingHandlers = true });
        }).Throws<MediatorException>();

        var actualMessage = ex.Data["Error:1"]?.ToString() ?? string.Empty;
        await Assert.That(actualMessage).IsEqualTo(MediatorExecutionException.CreateForNoHandler(typeof(InvalidActionWithoutHandler)).Message);
    }

    [Test]
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