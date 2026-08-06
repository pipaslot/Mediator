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
    /// Register action handler types. When <paramref name="serviceLifetime"/> is omitted (null), handlers implementing
    /// <see cref="ISingleton"/>/<see cref="IScoped"/> keep the lifetime dictated by that interface, and other handlers default
    /// to <see cref="ServiceLifetime.Transient"/>. When explicitly passed, it must match what <see cref="ISingleton"/>/<see cref="IScoped"/>
    /// require, or registration throws - this applies to an explicit <see cref="ServiceLifetime.Transient"/> as well.
    /// </summary>
    IMediatorConfigurator AddHandlers(IEnumerable<Type> handlerTypes, ServiceLifetime? serviceLifetime = null);

    /// <summary>
    /// Scan assemblies for action handler types
    /// </summary>
    IMediatorConfigurator AddHandlersFromAssembly(params Assembly[] assemblies);

    /// <summary>
    /// Will scan for action handlers from the assembly of type <typeparamref name="T"/> and register them.
    /// See <see cref="AddHandlers"/> for how <paramref name="serviceLifetime"/> interacts with <see cref="ISingleton"/>/<see cref="IScoped"/>.
    /// </summary>
    /// <typeparam name="T">The type from target asssembly to be scanned</typeparam>
    IMediatorConfigurator AddHandlersFromAssemblyOf<T>(ServiceLifetime? serviceLifetime = null);

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

    /// <summary>
    /// Register a typed exception handler translating exceptions caught at the Execute/Dispatch boundary into client-safe messages.
    /// The handled exception type(s) are discovered from the handler type's <see cref="IMediatorExceptionHandler{TException}"/> implementations -
    /// one handler type can cover several exception types, producing one routing entry per implemented interface.
    /// </summary>
    /// <exception cref="MediatorException">
    /// <typeparamref name="THandler"/> implements no <see cref="IMediatorExceptionHandler{TException}"/>, or a handler is already registered for one of its exception types.
    /// </exception>
    IMediatorConfigurator AddExceptionHandler<THandler>(ServiceLifetime lifetime = ServiceLifetime.Transient) where THandler : class;

    /// <summary>
    /// Register multiple typed exception handler types. See <see cref="AddExceptionHandler{THandler}"/> for the per-type rules.
    /// </summary>
    IMediatorConfigurator AddExceptionHandlers(IEnumerable<Type> handlerTypes, ServiceLifetime lifetime = ServiceLifetime.Transient);
}