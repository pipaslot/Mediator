using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Pipelines;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pipaslot.Mediator.Configuration;

public class MediatorConfigurator(IServiceCollection services) : IMediatorConfigurator, IActionTypeProvider, IMiddlewareResolver
{
    internal readonly IServiceCollection Services = services;
    internal HashSet<Assembly> TrustedAssemblies { get; set; } = [];
    private readonly List<Type> _actionMarkerTypes = [];
    private readonly MiddlewareCollection _middlewares = new(services);
    private readonly List<(IPipelineCondition Condition, MiddlewareCollection Middlewares, string Identifier)> _pipelines = [];

    /// <summary>
    /// Temporary storage used for handler configuration issue detection. Needs to be cleared once mediator is fully configured.
    /// </summary>
    private readonly Dictionary<Type, ServiceLifetime> _registeredHandlers = new();

    public void ClearTempData()
    {
        _registeredHandlers.Clear();
    }

    public IMediatorConfigurator AddActions(IEnumerable<Type> actionTypes)
    {
        var mediatorActionType = typeof(IMediatorAction);
        var actionTypeArray = actionTypes as Type[] ?? actionTypes.ToArray();
        foreach (var actionType in actionTypeArray)
        {
            if (!mediatorActionType.IsAssignableFrom(actionType))
            {
                throw MediatorException.CreateForNoActionType(actionType);
            }
        }

        _actionMarkerTypes.AddRange(actionTypeArray);
        TrustedAssemblies.UnionWith(actionTypeArray.Select(t => t.Assembly));
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
        var handlerArray = handlers as Type[] ?? handlers.ToArray();
        foreach (var handlerType in handlerArray)
        {
            var isHandler = handlerType.GetInterfaces()
                .Any(i => i.IsGenericType && handlerTypes.Contains(i.GetGenericTypeDefinition()));
            if (!isHandler)
            {
                throw MediatorException.CreateForNoHandlerType(handlerType);
            }
        }

        Services.RegisterHandlers(_registeredHandlers, handlerArray, serviceLifetime);
        return this;
    }

    public IMediatorConfigurator AddHandlersFromAssemblyOf<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
    {
        return RegisterHandlersFromAssembly([typeof(T).Assembly], serviceLifetime);
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

    public IMediatorConfigurator AddPipeline(IPipelineCondition condition, Action<IMiddlewareRegistrator> subMiddlewares,
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

    void IMiddlewareResolver.CollectMiddlewares(IMediatorAction action, IServiceProvider serviceProvider, List<Mediator.MiddlewarePair> collection)
    {
        CollectMiddlewares(action, serviceProvider, collection);
    }

    /// <summary>
    /// Resolve single pipeline and collect all middlewares
    /// </summary>
    /// <exception cref="MediatorException"></exception>
    internal void CollectMiddlewares(IMediatorAction action, IServiceProvider serviceProvider, List<Mediator.MiddlewarePair> collection)
    {
        if (_pipelines.Count > 0)
        {
            var matched = false;
            foreach (var pipeline in _pipelines)
            {
                if (pipeline.Condition.Matches(action))
                {
                    if (matched)
                    {
                        throw MediatorException.TooManyPipelines(action);
                    }
                    matched = true;
                    pipeline.Middlewares.CollectMiddlewares(action, serviceProvider, collection);
                }
            }

            if (matched)
            {
                return;
            }
        }

        _middlewares.CollectMiddlewares(action, serviceProvider, collection);
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