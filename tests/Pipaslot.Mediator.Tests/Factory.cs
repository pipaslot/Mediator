using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests
{
    internal class Factory
    {
        public static IMediator CreateConfiguredMediator()
        {
            return CreateConfiguredMediator(c => { });
        }

        public static IMediator CreateConfiguredMediator(Action<IMediatorConfigurator> setup)
        {
            var services = CreateServiceProvider(c =>
            {
                c.AddActionsFromAssemblyOf<Factory>()
                .AddActionsFromAssemblyOf<SingleHandler.Message>()
                .AddHandlersFromAssemblyOf<Factory>()
                .AddHandlersFromAssemblyOf<SingleHandler.MessageHandler>();
                setup(c);
            }
            );
            return services.GetRequiredService<IMediator>();
        }

        public static Mediator CreateInternalMediator(Action<IMediatorConfigurator> setup)
        {
            var services = CreateServiceProvider(c =>
            {
                setup(c);
            });
            return services.GetRequiredService<IMediator>() as Mediator;
        }

        public static IMediator CreateCustomMediator(Action<IMediatorConfigurator> setup)
        {
            var services = CreateServiceProvider(c =>
            {
                setup(c);
            });
            return services.GetRequiredService<IMediator>();
        }

        public static IServiceProvider CreateServiceProvider()
        {
            return CreateServiceProvider(_ => { });
        }

        public static MediatorConfigurator CreateMediatorConfigurator(Action<IMediatorConfigurator> setup)
        {
            var sp = CreateServiceProvider(setup);
            return sp.GetService<MediatorConfigurator>();
        }

        public static IServiceProvider CreateServiceProvider(Action<IMediatorConfigurator> setup)
        {
            var collection = new ServiceCollection();
            collection.AddLogging();
            setup(collection.AddMediator());
            return collection.BuildServiceProvider();
        }

        public static IServiceProvider CreateServiceProviderWithHandlers<THandler1>()
        {
            return CreateServiceProviderWithHandlers(typeof(THandler1));
        }

        public static IServiceProvider CreateServiceProviderWithHandlers<THandler1, THandler2>()
        {
            return CreateServiceProviderWithHandlers(typeof(THandler1), typeof(THandler2));
        }

        /// <summary>
        /// Simulate handler registration in service provider
        /// </summary>
        public static IServiceProvider CreateServiceProviderWithHandlers(params Type[] handlers)
        {
            var collection = new ServiceCollection();
            collection.RegisterHandlers(new Dictionary<Type, ServiceLifetime>(),handlers);
            return collection.BuildServiceProvider();
        }

        public static MiddlewareDelegate CreateMiddlewareDelegate()
        {
            return (MediatorContext ctx) => Task.CompletedTask;
        }

        public static MediatorContext FakeContext(IMediatorAction action)
        {
            var services = CreateServiceProvider();
            var mediator = new Mock<IMediator>();
            var sut = new HandlerExecutionMiddleware(services);
            var mcaMock = new Mock<IMediatorContextAccessor>();
            return new MediatorContext(mediator.Object, mcaMock.Object, services, action, CancellationToken.None, null, null);
        }
    }
}
