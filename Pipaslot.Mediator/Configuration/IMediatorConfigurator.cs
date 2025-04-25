using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares.Pipelines;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Pipaslot.Mediator.Configuration;

/// <summary>
/// Commont configuration for all pipelines and for handler processing. Scans assemblies for action markers and their handlers. Pipeline is specified by registered middlewares by their order
/// </summary>
public interface IMediatorConfigurator : IMiddlewareRegistrator
{
    /// <summary>
    /// Register action handler types
    /// </summary>
    IMediatorConfigurator AddHandlers(IEnumerable<Type> handlerTypes, ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

    /// <summary>
    /// Scan assemblies for action handler types
    /// </summary>
    IMediatorConfigurator AddHandlersFromAssembly(params Assembly[] assemblies);

    /// <summary>
    /// Will scan for action handlers from the assembly of type <typeparamref name="T"/> and register them.
    /// </summary>
    /// <typeparam name="T">The type from target asssembly to be scanned</typeparam>
    IMediatorConfigurator AddHandlersFromAssemblyOf<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

    /// <summary>
    /// Register action types
    /// </summary>
    IMediatorConfigurator AddActions(IEnumerable<Type> actionTypes);

    /// <summary>
    /// Will scan for action markers from the passed assemblies and register them.
    /// </summary>
    IMediatorConfigurator AddActionsFromAssembly(params Assembly[] assemblies);

    /// <summary>
    /// Will scan for action markers from the assembly of type <typeparamref name="T"/> and register them.
    /// </summary>
    /// <typeparam name="T">The type from target assembly to be scanned</typeparam>
    IMediatorConfigurator AddActionsFromAssemblyOf<T>();

    /// <summary>
    /// Register middlewares as pipeline executed independently of the default pipeline
    /// </summary>
    /// <param name="condition"></param>
    /// <param name="subMiddlewares">Middlewares applied when condition is met</param>
    /// <param name="identifier">Customized unique pipeline identifier. Pipeline with the same identifier will be replaced</param>
    IMediatorConfigurator AddPipeline(IPipelineCondition condition, Action<IMiddlewareRegistrator> subMiddlewares,
        string? identifier = null);
}