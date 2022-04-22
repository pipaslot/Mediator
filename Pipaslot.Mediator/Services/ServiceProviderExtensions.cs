using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Services
{
    internal static class ServiceProviderExtensions
    {
        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        internal static object[] GetMessageHandlers(this IServiceProvider serviceProvider, Type? messageType)
        {
            if (messageType == null)
            {
                return new object[0];
            }
            var mediatorActionType = typeof(IMediatorAction);
            var handlerType = typeof(IMediatorHandler<>).MakeGenericType(messageType);
            return serviceProvider.GetServices(handlerType)
                .Where(h => h != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<object>()
                .ToArray();
        }

        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        internal static object[] GetRequestHandlers(this IServiceProvider serviceProvider, Type? requestType, Type? responseType)
        {
            if (requestType == null || responseType == null)
            {
                return new object[0];
            }
            var mediatorHandlerType = typeof(IMediatorHandler<,>);
            var handlerType = mediatorHandlerType.MakeGenericType(requestType, responseType);
            return serviceProvider.GetServices(handlerType)
                .Where(h => h != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<object>()
                .ToArray();
        }

        internal static void RegisterHandlers(this IServiceCollection services, IEnumerable<Type> allTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            var handlerTypes = new[]
            {
                typeof(IMediatorHandler<,>),
                typeof(IMediatorHandler<>)
            };
            var types = allTypes
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
                    var item = new ServiceDescriptor(iface, pair.Type, serviceLifetime);
                    services.Add(item);
                }
            }
        }
    }
}