using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E
{
    public class HandlerLifetimes
    {
        #region Manually handled lifecycle

        [Theory]
        [InlineData(2, ServiceLifetime.Transient)]
        [InlineData(1, ServiceLifetime.Scoped)]
        [InlineData(1, ServiceLifetime.Singleton)]
        public async Task NoInterface_Registration_ShareTheSameHandlerInstance(int expectedInstanceCount, ServiceLifetime lifetime)
        {
            var sut = Factory.CreateCustomMediator(c => c
                .AddActions(new[] { typeof(InstanceCounterMessage) })
                .AddHandlers(new[] { typeof(InstanceCounterMessageHandler) }, lifetime)
            );
            InstanceCounterMessageHandler.Instances.Clear();
            await sut.Dispatch(new InstanceCounterMessage());
            await sut.Dispatch(new InstanceCounterMessage());
            Assert.Equal(expectedInstanceCount, InstanceCounterMessageHandler.Instances.Count);
        }

        [Fact]
        public void MixedReRegistration_Pass()
        {
            var handlerType = typeof(SingletorMessageHandler);
            Factory.CreateCustomMediator(c => c
                    .AddHandlers(new[] { handlerType })
                    .AddHandlersFromAssemblyOf<SingletorMessageHandler>()//There is als othe scoped
                    .AddHandlers(new[] { handlerType })
                 );
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient, ServiceLifetime.Scoped)]
        [InlineData(ServiceLifetime.Scoped, ServiceLifetime.Singleton)]
        [InlineData(ServiceLifetime.Singleton, ServiceLifetime.Transient)]
        public void ReRegistrationWithDifferentScope_ThrowException(ServiceLifetime initial, ServiceLifetime update)
        {
            var handlerType = typeof(NopMesageHandler);
            var ex = Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateCustomMediator(c => c
                    .AddHandlers(new[] { handlerType }, initial)
                    .AddHandlers(new[] { handlerType }, update)
                );
            });
            Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, initial, update).Message, ex.Message);
        }

        #endregion

        #region ISingleton inteface applied to the handler class

        [Fact]
        public async Task ISingletonInterface_AutomaticRegistration_ShareTheSameHandlerInstance()
        {
            var sut = Factory.CreateCustomMediator(c => c
                .AddActionsFromAssemblyOf<SingletorMessage>()
                .AddHandlersFromAssemblyOf<SingletorMessageHandler>()
                );
            SingletorMessageHandler.Instances.Clear();
            await sut.Dispatch(new SingletorMessage());
            await sut.Dispatch(new SingletorMessage());
            Assert.Single(SingletorMessageHandler.Instances);
        }

        [Theory]
        //[InlineData(ServiceLifetime.Transient)] TODO: add this case in next major version. Currently the mediator is not able to distinguish if the Transient is default or not
        [InlineData(ServiceLifetime.Scoped)]
        public void ISingletonInterface_Registration_ThrowException(ServiceLifetime lifetime)
        {
            var handlerType = typeof(SingletorMessageHandler);
            var ex = Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateCustomMediator(c => c
                    .AddHandlers(new[] { handlerType }, lifetime)
                );
            });

            Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, ServiceLifetime.Singleton, lifetime).Message, ex.Message);
        }

        [Fact]
        public void ISingletonInterface_ReRegistration_Pass()
        {
            var handlerType = typeof(SingletorMessageHandler);
            Factory.CreateCustomMediator(c => c
                    .AddHandlers(new[] { handlerType })
                    .AddHandlers(new[] { handlerType })
                 );
        }

        [Fact]
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

        [Theory]
        //[InlineData(ServiceLifetime.Transient)] TODO: add this case in next major version. Currently the mediator is not able to distinguish if the Transient is default or not
        [InlineData(ServiceLifetime.Singleton)]
        public void IScopedInterface_Registration_ThrowException(ServiceLifetime lifetime)
        {
            var handlerType = typeof(ScopedMessageHandler);
            var ex = Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateCustomMediator(c => c
                    .AddHandlers(new[] { handlerType }, lifetime)
                );
            });

            Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, ServiceLifetime.Scoped, lifetime).Message, ex.Message);
        }

        [Fact]
        public void IScopedInterface_ReRegistration_Pass()
        {
            var handlerType = typeof(ScopedMessageHandler);
            Factory.CreateCustomMediator(c => c
                    .AddHandlers(new[] { handlerType })
                    .AddHandlers(new[] { handlerType })
                 );
        }

        #endregion
    }
}
