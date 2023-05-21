﻿using Microsoft.Extensions.DependencyInjection;
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
        public async Task NoInterface_TypeRegistration_ShareTheSameHandlerInstance(int expectedInstanceCount, ServiceLifetime lifetime)
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

        [Theory]
        [InlineData(2, ServiceLifetime.Transient)]
        [InlineData(1, ServiceLifetime.Scoped)]
        [InlineData(1, ServiceLifetime.Singleton)]
        public async Task NoInterface_AssemblyRegistration_ShareTheSameHandlerInstance(int expectedInstanceCount, ServiceLifetime lifetime)
        {
            var sut = Factory.CreateCustomMediator(c => c
                .AddActionsFromAssemblyOf<InstanceCounterMessage>()
                .AddHandlersFromAssemblyOf<InstanceCounterMessageHandler>(lifetime)
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
            Factory.CreateConfiguredMediator(c => c
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
                Factory.CreateConfiguredMediator(c => c
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
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped)]
        public void ISingletonInterface_TypeRegistration_ThrowException(ServiceLifetime lifetime)
        {
            var handlerType = typeof(SingletorMessageHandler);
            var ex = Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateConfiguredMediator(c => c
                    .AddHandlers(new[] { handlerType }, lifetime)
                );
            });

            Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, ServiceLifetime.Singleton,lifetime).Message, ex.Message);
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Scoped)]
        public void ISingletonInterface_AssemblyRegistration_ThrowException(ServiceLifetime lifetime)
        {
            var handlerType = typeof(SingletorMessageHandler);
            var ex = Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateConfiguredMediator(c => c
                    .AddHandlers(new[] { handlerType }, lifetime)
                );
            });

            Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, ServiceLifetime.Singleton, lifetime).Message, ex.Message);
        }

        [Fact]
        public void ISingletonInterface_AssemblyReRegistration_Pass()
        {
            Factory.CreateConfiguredMediator(c => c
                    .AddHandlersFromAssemblyOf<SingletorMessageHandler>()
                    .AddHandlersFromAssemblyOf<SingletorMessageHandler>()
                );
        }

        [Fact]
        public void ISingletonInterface_TypeReRegistration_Pass()
        {
            var handlerType = typeof(SingletorMessageHandler);
            Factory.CreateConfiguredMediator(c => c
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
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        public void IScopedInterface_TypeRegistration_ThrowException(ServiceLifetime lifetime)
        {
            var handlerType = typeof(ScopedMessageHandler);
            var ex = Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateConfiguredMediator(c => c
                    .AddHandlers(new[] { handlerType }, lifetime)
                );
            });

            Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, ServiceLifetime.Scoped, lifetime).Message, ex.Message);
        }

        [Theory]
        [InlineData(ServiceLifetime.Transient)]
        [InlineData(ServiceLifetime.Singleton)]
        public void IScopedInterface_AssemblyRegistration_ThrowException(ServiceLifetime lifetime)
        {
            var handlerType = typeof(ScopedMessageHandler);
            var ex = Assert.Throws<MediatorException>(() =>
            {
                Factory.CreateConfiguredMediator(c => c
                    .AddHandlers(new[] { handlerType }, lifetime)
                );
            });

            Assert.Equal(MediatorException.CreateForWrongHandlerServiceLifetime(handlerType, ServiceLifetime.Scoped, lifetime).Message, ex.Message);
        }

        [Fact]
        public void IScopedInterface_AssemblyReRegistration_Pass()
        {
            Factory.CreateConfiguredMediator(c => c
                    .AddHandlersFromAssemblyOf<ScopedMessageHandler>()
                    .AddHandlersFromAssemblyOf<ScopedMessageHandler>()
                );
        }

        [Fact]
        public void IScopedInterface_TypeReRegistration_Pass()
        {
            var handlerType = typeof(ScopedMessageHandler);
            Factory.CreateConfiguredMediator(c => c
                    .AddHandlers(new[] { handlerType })
                    .AddHandlers(new[] { handlerType })
                 );
        }

        #endregion
    }
}
