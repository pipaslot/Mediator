using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Configuration;

internal class MiddlewareCollection(IServiceCollection services) : IMiddlewareResolver, IMiddlewareRegistrator
{
    private readonly List<IMiddlewareResolver> _middlewareTypes = [];

    private void AddMiddleware(Type middlewareType, ServiceLifetime lifetime, object[]? parameters = null)
    {
        _middlewareTypes.Add(new MiddlewareDefinition(middlewareType, parameters));
        var existingDescriptor = services.FirstOrDefault(d => d.ServiceType == middlewareType && d.ImplementationType == middlewareType);
        if (existingDescriptor != null)
        {
            if (existingDescriptor.Lifetime != lifetime)
            {
                throw new MediatorException(
                    $"Can not register the same middleware with different ServiceLifetime. Service {middlewareType} was already registered with ServiceLifetime {existingDescriptor.Lifetime}.");
            }
        }
        else
        {
            services.Add(new ServiceDescriptor(middlewareType, middlewareType, lifetime));
        }
    }

    public void CollectMiddlewares(IMediatorAction action, IServiceProvider serviceProvider, List<Mediator.MiddlewarePair> collection)
    {
        foreach (var res in _middlewareTypes)
        {
            res.CollectMiddlewares(action, serviceProvider, collection);
        }
    }

    public IMiddlewareRegistrator Use<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Scoped, object[]? parameters = null)
        where TMiddleware : IMediatorMiddleware
    {
        AddMiddleware(typeof(TMiddleware), lifetime, parameters);
        return this;
    }

    public IMiddlewareRegistrator Use<TMiddleware>(Action<IServiceCollection> setupDependencies, ServiceLifetime lifetime = ServiceLifetime.Scoped,
        object[]? parameters = null) where TMiddleware : IMediatorMiddleware
    {
        setupDependencies(services);
        AddMiddleware(typeof(TMiddleware), lifetime, parameters);
        return this;
    }

    public IMiddlewareRegistrator UseWhen(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares)
    {
        var config = new MiddlewareCollection(services);
        var definition = new ConditionDefinition(condition, config);
        _middlewareTypes.Add(definition);
        subMiddlewares(config);
        return this;
    }

    public IMiddlewareRegistrator UseWhen(Func<IMediatorAction, IServiceProvider, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares)
    {
        var config = new MiddlewareCollection(services);
        var definition = new DynamicDefinition(condition, config);
        _middlewareTypes.Add(definition);
        subMiddlewares(config);
        return this;
    }


    private class ConditionDefinition(Func<IMediatorAction, bool> condition, MiddlewareCollection middlewares) : IMiddlewareResolver
    {
        public void CollectMiddlewares(IMediatorAction action, IServiceProvider serviceProvider, List<Mediator.MiddlewarePair> collection)
        {
            if (condition(action))
            {
                middlewares.CollectMiddlewares(action, serviceProvider, collection);
            }
        }
    }

    private class DynamicDefinition(Func<IMediatorAction, IServiceProvider, bool> condition, MiddlewareCollection middlewares)
        : IMiddlewareResolver
    {
        public void CollectMiddlewares(IMediatorAction action, IServiceProvider serviceProvider, List<Mediator.MiddlewarePair> collection)
        {
            if (condition(action, serviceProvider))
            {
                middlewares.CollectMiddlewares(action, serviceProvider, collection);
            }
        }
    }
}