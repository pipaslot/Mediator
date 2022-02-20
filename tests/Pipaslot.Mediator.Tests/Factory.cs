using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests
{
    internal class Factory
    {
        public static IMediator CreateMediator()
        {
            return CreateMediator(c => { });
        }
        public static IMediator CreateMediator(Action<IMediatorConfigurator> setup)
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
        public static IServiceProvider CreateServiceProvider()
        {
            return CreateServiceProvider(_ => { });
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
            var services = (ICollection<ServiceDescriptor>)collection;
            var handlerTypes = new[]
            {
                typeof(IMediatorHandler<,>),
                typeof(IMediatorHandler<>)
            };
            var types = handlers
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && handlerTypes.Contains(i.GetGenericTypeDefinition())))
                .Select(t => new
                {
                    Type = t,
                    Interfaces = t.GetInterfaces()
                        .Where(i => i.IsGenericType && handlerTypes.Contains(i.GetGenericTypeDefinition()))
                });
            foreach (var pair in types)
            {
                foreach (var iface in pair.Interfaces)
                {
                    var item = new ServiceDescriptor(iface, pair.Type, ServiceLifetime.Scoped);
                    services.Add(item);
                }
            }
            return collection.BuildServiceProvider();
        }

        public static MiddlewareDelegate CreateMiddlewareDelegate()
        {
            return (MediatorContext ctx) => Task.CompletedTask;
        }
    }
}
