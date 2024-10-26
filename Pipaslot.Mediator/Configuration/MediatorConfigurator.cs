﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pipaslot.Mediator.Configuration;

public class MediatorConfigurator : IMediatorConfigurator, IActionTypeProvider, IMiddlewareResolver
{
    internal readonly IServiceCollection Services;
    internal HashSet<Assembly> TrustedAssemblies { get; set; } = new();
    private List<Type> _actionMarkerTypes = new();
    private MiddlewareCollection _middlewares;
    private List<(Func<IMediatorAction, bool> Condition, MiddlewareCollection Middlewares, string Identifier)> _pipelines = new();

    /// <summary>
    /// Temporary storage used for handler configuration issue detection. Needs to be cleared once mediator is fully configured.
    /// </summary>
    private Dictionary<Type, ServiceLifetime> _registeredHandlers = new();

    public MediatorConfigurator(IServiceCollection services)
    {
        Services = services;
        _middlewares = new MiddlewareCollection(services);
    }

    public void ClearTempData()
    {
        _registeredHandlers.Clear();
    }

    public IMediatorConfigurator AddActions(IEnumerable<Type> actionTypes)
    {
        var mediatorActionType = typeof(IMediatorAction);
        foreach (var actionType in actionTypes)
        {
            if (!mediatorActionType.IsAssignableFrom(actionType))
            {
                throw MediatorException.CreateForNoActionType(actionType);
            }
        }

        _actionMarkerTypes.AddRange(actionTypes);
        TrustedAssemblies.UnionWith(actionTypes.Select(t => t.Assembly));
        return this;
    }

    public IMediatorConfigurator AddActionsFromAssemblyOf<T>()
    {
        return AddActionsFromAssembly(typeof(T).Assembly);
    }

    public IMediatorConfigurator AddActionsFromAssembly(params Assembly[] assemblies)
    {
        var type = typeof(IMediatorAction);
        var actionTypes = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(p => p.IsClass
                        && !p.IsAbstract
                        && !p.IsInterface
                        && p.GetInterfaces().Any(i => i == type)
            )
            .ToArray();
        _actionMarkerTypes.AddRange(actionTypes);
        TrustedAssemblies.UnionWith(assemblies);
        return this;
    }

    public IMediatorConfigurator AddHandlers(IEnumerable<Type> handlers, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var handlerTypes = new[] { typeof(IMediatorHandler<,>), typeof(IMediatorHandler<>) };
        foreach (var handlerType in handlers)
        {
            var isHandler = handlerType.GetInterfaces()
                .Any(i => i.IsGenericType && handlerTypes.Contains(i.GetGenericTypeDefinition()));
            if (!isHandler)
            {
                throw MediatorException.CreateForNoHandlerType(handlerType);
            }
        }

        Services.RegisterHandlers(_registeredHandlers, handlers, serviceLifetime);
        return this;
    }

    public IMediatorConfigurator AddHandlersFromAssemblyOf<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        return RegisterHandlersFromAssembly(new[] { typeof(T).Assembly }, serviceLifetime);
    }

    public IMediatorConfigurator AddHandlersFromAssembly(params Assembly[] assemblies)
    {
        return RegisterHandlersFromAssembly(assemblies);
    }

    private IMediatorConfigurator RegisterHandlersFromAssembly(Assembly[] assemblies, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        var types = assemblies.SelectMany(a => a.GetTypes());
        Services.RegisterHandlers(_registeredHandlers, types, serviceLifetime);
        return this;
    }

    public IMiddlewareRegistrator Use<TMiddleware>(Action<IServiceCollection> setupDependencies, ServiceLifetime lifetime = ServiceLifetime.Scoped,
        object[]? parameters = null) where TMiddleware : IMediatorMiddleware
    {
        return _middlewares.Use<TMiddleware>(setupDependencies, lifetime, parameters);
    }

    public IMiddlewareRegistrator Use<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Scoped, object[]? parameters = null)
        where TMiddleware : IMediatorMiddleware
    {
        return _middlewares.Use<TMiddleware>(lifetime, parameters);
    }

    public IMiddlewareRegistrator UseWhen(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares)
    {
        _middlewares.UseWhen(condition, subMiddlewares);
        return this;
    }

    public IMiddlewareRegistrator UseWhen(Func<IMediatorAction, IServiceProvider, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares)
    {
        _middlewares.UseWhen(condition, subMiddlewares);
        return this;
    }

    public IMediatorConfigurator AddPipeline(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares,
        string? identifier = null)
    {
        var collection = new MiddlewareCollection(Services);
        subMiddlewares(collection);
        if (identifier != null)
        {
            _pipelines.RemoveAll(p => p.Identifier == identifier);
        }

        var id = identifier ?? Guid.NewGuid().ToString();
        _pipelines.Add((condition, collection, id));
        return this;
    }

    public ICollection<Type> GetActionTypes()
    {
        return _actionMarkerTypes;
    }

    public ICollection<Type> GetMessageActionTypes()
    {
        return FilterAssignableToMessage(_actionMarkerTypes);
    }

    public ICollection<Type> GetRequestActionTypes()
    {
        return FilterAssignableToRequest(_actionMarkerTypes);
    }

    IEnumerable<MiddlewareDefinition> IMiddlewareResolver.GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
    {
        return GetMiddlewares(action, serviceProvider);
    }

    internal IEnumerable<MiddlewareDefinition> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
    {
        var pipelines = _pipelines
            .Where(p => p.Condition(action))
            .ToArray();
        if (pipelines.Length > 1)
        {
            throw MediatorException.TooManyPipelines(action);
        }
        else if (pipelines.Length == 1)
        {
            return pipelines.First().Middlewares.GetMiddlewares(action, serviceProvider);
        }

        return _middlewares.GetMiddlewares(action, serviceProvider);
    }

    internal static Type[] FilterAssignableToRequest(IEnumerable<Type> types)
    {
        var genericRequestType = typeof(IMediatorAction<>);
        return types
            .Where(t => t.IsClass
                        && !t.IsAbstract
                        && !t.IsInterface
                        && t.GetInterfaces()
                            .Any(i => i.IsGenericType
                                      && i.GetGenericTypeDefinition() == genericRequestType)
            )
            .ToArray();
    }

    internal static Type[] FilterAssignableToMessage(IEnumerable<Type> types)
    {
        var genericRequestType = typeof(IMediatorAction<>);
        var type = typeof(IMediatorAction);
        return types
            .Where(p => p.IsClass
                        && !p.IsAbstract
                        && !p.IsInterface
                        && p.GetInterfaces().Any(i => i == type)
                        && !p.GetInterfaces()
                            .Any(i => i.IsGenericType
                                      && i.GetGenericTypeDefinition() == genericRequestType)
            )
            .ToArray();
    }
}