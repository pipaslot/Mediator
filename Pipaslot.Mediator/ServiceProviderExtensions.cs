using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// Resolve all action handlers
    /// </summary>
    public static object[] GetActionHandlers(this IServiceProvider serviceProvider, IMediatorAction action)
    {
        var actionType = action.GetType();
        if (action is IMediatorActionProvidingData)
        {
            var resultType = RequestGenericHelpers.GetRequestResultType(actionType);
            return serviceProvider.GetRequestHandlers(actionType, resultType);
        }

        return serviceProvider.GetMessageHandlers(actionType);
    }

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

    internal static void RegisterHandlers(this IServiceCollection services, Dictionary<Type, ServiceLifetime> registeredHandler,
        IEnumerable<Type> allTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var handlerTypes = new[] { typeof(IMediatorHandler<,>), typeof(IMediatorHandler<>) };
        var singletonType = typeof(ISingleton);
        var scopedType = typeof(IScoped);
        var types = allTypes
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .Select(t =>
            {
                var interfaces = t.GetInterfaces();
                return new
                {
                    Type = t,
                    AllInterfaces = interfaces,
                    Interfaces = interfaces
                        .Where(i => i.IsGenericType && handlerTypes.Contains(i.GetGenericTypeDefinition()))
                        .ToArray(),
                    Lifetime = interfaces.Contains(singletonType)
                        ? ServiceLifetime.Singleton
                        : interfaces.Contains(scopedType)
                            ? ServiceLifetime.Scoped
                            : serviceLifetime
                };
            })
            .Where(t => t.Interfaces.Any());
        foreach (var pair in types)
        {
            if (pair.Lifetime != serviceLifetime)
            {
                // Only throw when not the default one
                // TODO We should consider to change the serviceLifetime type to nullable and do the same also on interfaces in next major version
                if (serviceLifetime != ServiceLifetime.Transient)
                {
                    throw MediatorException.CreateForWrongHandlerServiceLifetime(pair.Type, pair.Lifetime, serviceLifetime);
                }
            }

            if (registeredHandler.TryGetValue(pair.Type, out var existingLifetime))
            {
                if (existingLifetime != pair.Lifetime)
                {
                    throw MediatorException.CreateForWrongHandlerServiceLifetime(pair.Type, existingLifetime, pair.Lifetime);
                }
            }
            else
            {
                registeredHandler[pair.Type] = pair.Lifetime;
            }

            foreach (var iface in pair.Interfaces)
            {
                var item = new ServiceDescriptor(iface, pair.Type, pair.Lifetime);
                services.Add(item);
            }
        }
    }
}