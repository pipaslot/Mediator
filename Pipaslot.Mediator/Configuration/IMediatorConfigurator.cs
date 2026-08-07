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
    /// <remarks>
    /// A replacement, not an addition: an action matching this condition runs these middlewares instead of the
    /// ones registered with <see cref="IMiddlewareRegistrator.Use{TMiddleware}(ServiceLifetime,object[])"/> on the
    /// configurator, so anything the action still needs has to be repeated here. The default pipeline applies only to
    /// actions that no named pipeline matched.
    /// <para>
    /// At most one pipeline may match a given action - a second match throws <see cref="MediatorException"/> at dispatch
    /// time, not at startup, so overlapping conditions stay invisible until that action is executed. Prefer disjoint
    /// conditions (marker interfaces rather than overlapping predicates), and pass <paramref name="identifier"/> when a
    /// later registration is meant to replace an earlier one instead of competing with it.
    /// </para>
    /// <para>
    /// To add a middleware for a subset of actions while keeping the shared pipeline, use
    /// <see cref="IMiddlewareRegistrator.UseWhen(Func{Abstractions.IMediatorAction,bool},Action{IMiddlewareRegistrator})"/> instead.
    /// See docs/wiki/6.-Pipelines-and-Middlewares.md.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// services.AddMediator()
    ///     .Use&lt;LoggingMiddleware&gt;()                        // default pipeline, for actions matching no condition below
    ///     .AddPipelineForAction&lt;IMessage&gt;(p =&gt; p            // replaces the default pipeline for every IMessage
    ///         .Use&lt;LoggingMiddleware&gt;()
    ///         .Use&lt;ValidationMiddleware&gt;());
    /// </code>
    /// </example>
    /// <param name="condition">Decides whether this pipeline handles the dispatched action</param>
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