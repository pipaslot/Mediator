using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares.Handlers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

[Obsolete("The class will be set as internal in future versions.")]
public static class ServiceProviderExtensions
{
    internal static HandlerExecutor GetHandlerExecutor(this IServiceProvider services, Type actionType)
    {
        var configurator = services.GetRequiredService<MediatorConfigurator>();
        var executorType = configurator.ReflectionCache.GetHandlerExecutorType(actionType);
        return (HandlerExecutor)services.GetRequiredService(executorType);
    }
    
    internal static HandlerExecutor GetHandlerExecutor(this IServiceProvider services, ReflectionCache reflectionCache, Type actionType)
    {
        var executorType = reflectionCache.GetHandlerExecutorType(actionType);
        return (HandlerExecutor)services.GetRequiredService(executorType);
    }

    /// <summary>
    /// Resolves the exception handler executor for the most specific registered handler matching <paramref name="concreteExceptionType"/>,
    /// or null when no registered handler matches.
    /// </summary>
    internal static ExceptionHandlerExecutor? GetExceptionHandlerExecutor(this IServiceProvider services, ExceptionHandlerCache exceptionHandlerCache, Type concreteExceptionType)
    {
        var entry = exceptionHandlerCache.Resolve(concreteExceptionType);
        if (entry is null)
        {
            return null;
        }

        return (ExceptionHandlerExecutor)services.GetRequiredService(entry.ExecutorType);
    }

    /// <summary>
    /// Get all registered handlers from service provider
    /// </summary>
    internal static object[] GetMessageHandlers(this IServiceProvider serviceProvider, Type? messageType)
    {
        if (messageType == null)
        {
            return [];
        }

        var handlerType = typeof(IMediatorHandler<>).MakeGenericType(messageType);// TODO get rid of
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
            return [];
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