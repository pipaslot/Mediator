using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Tests.ValidActions;

namespace Pipaslot.Mediator.Tests.E2E;

public class HandlerLifetimes
{
    #region Manually handled lifecycle

    [Test]
    [Arguments(2, ServiceLifetime.Transient)]
    [Arguments(1, ServiceLifetime.Scoped)]
    [Arguments(1, ServiceLifetime.Singleton)]
    public async Task NoInterface_Registration_ShareTheSameHandlerInstance(int expectedInstanceCount, ServiceLifetime lifetime)
    {
        var sut = Factory.CreateCustomMediator(c => c
            .AddActions([typeof(InstanceCounterMessage)])
            .AddHandlers([typeof(InstanceCounterMessageHandler)], lifetime)
        );
        InstanceCounterMessageHandler.Instances.Clear();
        await sut.Dispatch(new InstanceCounterMessage());
        await sut.Dispatch(new InstanceCounterMessage());
        Assert.Equal(expectedInstanceCount, InstanceCounterMessageHandler.Instances.Count);
    }

    [Test]
    public void MixedReRegistration_Pass()
    {
        var handlerType = typeof(SingletonMessageHandler);
        Factory.CreateCustomMediator(c => c
            .AddHandlers([handlerType])
            .AddHandlersFromAssemblyOf<SingletonMessageHandler>() //There is als othe scoped
            .AddHandlers([handlerType])
        );
    }

    [Test]
    [Arguments(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
    [Arguments(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
    [Arguments(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
    public void ReRegistrationWithDifferentScope_ThrowException(ServiceLifetime initial, ServiceLifetime update)
    {
        var handlerType = typeof(NopMesageHandler);
        var ex = Assert.Throws<MediatorException>(() =>
        {
            Factory.CreateCustomMediator(c => c
                .AddHandlers([handlerType], initial)
                .AddHandlers([handlerType], update)
            );
        });
        Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, initial, update).Message, ex.Message);
    }

    #endregion

    #region ISingleton inteface applied to the handler class

    [Test]
    public async Task ISingletonInterface_AutomaticRegistration_ShareTheSameHandlerInstance()
    {
        var sut = Factory.CreateCustomMediator(c => c
            .AddActionsFromAssemblyOf<SingletonMessage>()
            .AddHandlersFromAssemblyOf<SingletonMessageHandler>()
        );
        SingletonMessageHandler.Instances.Clear();
        await sut.Dispatch(new SingletonMessage());
        await sut.Dispatch(new SingletonMessage());
        Assert.Single(SingletonMessageHandler.Instances);
    }

    [Test]
    //[Arguments(ServiceLifetime.Transient)] TODO: add this case in next major version. Currently the mediator is not able to distinguish if the Transient is default or not
    [Arguments(ServiceLifetime.Scoped)]
    public void ISingletonInterface_Registration_ThrowException(ServiceLifetime lifetime)
    {
        var handlerType = typeof(SingletonMessageHandler);
        var ex = Assert.Throws<MediatorException>(() =>
        {
            Factory.CreateCustomMediator(c => c
                .AddHandlers([handlerType], lifetime)
            );
        });

        Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, ServiceLifetime.Singleton, lifetime).Message, ex.Message);
    }

    [Test]
    public void ISingletonInterface_ReRegistration_Pass()
    {
        var handlerType = typeof(SingletonMessageHandler);
        Factory.CreateCustomMediator(c => c
            .AddHandlers([handlerType])
            .AddHandlers([handlerType])
        );
    }

    [Test]
    public async Task IScopedInterface_AutomaticRegistration_ShareTheSameHandlerInstance()
    {
        var sut = Factory.CreateCustomMediator(c => c
            .AddActionsFromAssemblyOf<ScopedMessage>()
            .AddHandlersFromAssemblyOf<ScopedMessageHandler>()
        );
        ScopedMessageHandler.Instances.Clear();
        await sut.Dispatch(new ScopedMessage());
        await sut.Dispatch(new ScopedMessage());
        Assert.Single(ScopedMessageHandler.Instances);
    }

    [Test]
    //[Arguments(ServiceLifetime.Transient)] TODO: add this case in next major version. Currently the mediator is not able to distinguish if the Transient is default or not
    [Arguments(ServiceLifetime.Singleton)]
    public void IScopedInterface_Registration_ThrowException(ServiceLifetime lifetime)
    {
        var handlerType = typeof(ScopedMessageHandler);
        var ex = Assert.Throws<MediatorException>(() =>
        {
            Factory.CreateCustomMediator(c => c
                .AddHandlers([handlerType], lifetime)
            );
        });

        Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, ServiceLifetime.Scoped, lifetime).Message, ex.Message);
    }

    [Test]
    public void IScopedInterface_ReRegistration_Pass()
    {
        var handlerType = typeof(ScopedMessageHandler);
        Factory.CreateCustomMediator(c => c
            .AddHandlers([handlerType])
            .AddHandlers([handlerType])
        );
    }

    #endregion
}