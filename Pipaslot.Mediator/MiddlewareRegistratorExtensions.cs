using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using Pipaslot.Mediator.Notifications;
using System;
using System.Linq;

namespace Pipaslot.Mediator;

/// <summary>
/// Shorthands for <see cref="IMiddlewareRegistrator.Use{TMiddleware}(ServiceLifetime, object[])"/> and <see cref="IMiddlewareRegistrator.UseWhen(Func{IMediatorAction, bool}, Action{IMiddlewareRegistrator})"/>:
/// middlewares with parameters, conditions on action marker types, and the middlewares shipped with the library.
/// </summary>
/// <remarks>
/// The <c>UseWhen*</c> methods add a branch to the pipeline they are called on - the surrounding middlewares still run.
/// To give a set of actions a different pipeline instead, use <see cref="MediatorConfiguratorExtensions"/>.
/// </remarks>
public static class MiddlewareRegistratorExtensions
{
    /// <summary>
    /// Register middleware in pipeline for all actions and propagate parameters to the middleware. 
    /// <para>The parameters are available in <see cref="MediatorContext.Features"/> under type <see cref="MiddlewareParametersFeature"/></para>
    /// </summary>
    /// <remarks>
    /// Registers the middleware as scoped. The same middleware type can be registered several times with different
    /// parameters, each registration seeing only its own.
    /// </remarks>
    /// <typeparam name="TMiddleware">Middleware type</typeparam>
    /// <param name="configurator">The pipeline to add the middleware to</param>
    /// <param name="parameters">Parameters passed to <see cref="MediatorContext.Features"/> right before middleware execution and available under type <see cref="MiddlewareParametersFeature"/></param>
    public static IMiddlewareRegistrator UseWithParameters<TMiddleware>(this IMiddlewareRegistrator configurator, params object[] parameters)
        where TMiddleware : IMediatorMiddleware
    {
        return configurator.Use<TMiddleware>(ServiceLifetime.Scoped, parameters);
    }

    /// <summary>
    /// Register middleware in pipeline for all actions. 
    /// <para>The parameters are available in <see cref="MediatorContext.Features"/> under type <see cref="MiddlewareParametersFeature"/></para>
    /// </summary>
    /// <typeparam name="TMiddleware">Middleware type</typeparam>
    /// <param name="configurator">The pipeline to add the middleware to</param>
    /// <param name="setupDependencies">Additional dependencies registered with middleware</param>
    /// <param name="parameters">Parameters passed to <see cref="MediatorContext.Features"/> right before middleware execution and available under type <see cref="MiddlewareParametersFeature"/></param>
    public static IMiddlewareRegistrator UseWithParameters<TMiddleware>(this IMiddlewareRegistrator configurator,
        Action<IServiceCollection> setupDependencies, params object[] parameters) where TMiddleware : IMediatorMiddleware
    {
        return configurator.Use<TMiddleware>(setupDependencies, ServiceLifetime.Scoped, parameters);
    }

    /// <summary>
    /// Register middleware applied only for actions passing the condition.
    /// </summary>
    /// <remarks>
    /// The condition is evaluated on every execution - keep it cheap, typically a type check.
    /// </remarks>
    public static IMiddlewareRegistrator UseWhen<TMiddleware>(this IMiddlewareRegistrator configurator, Func<IMediatorAction, bool> condition)
        where TMiddleware : IMediatorMiddleware
    {
        return configurator.UseWhen(condition, m => m.Use<TMiddleware>());
    }

    #region UseWhenAction

    /// <summary>
    /// Register <typeparamref name="TMiddleware"/> applied only for actions implementing <typeparamref name="TActionMarker"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenAction<TActionMarker, TMiddleware>(this IMiddlewareRegistrator configurator)
        where TMiddleware : IMediatorMiddleware
    {
        return configurator.UseWhenAction<TActionMarker>(m => m.Use<TMiddleware>());
    }

    /// <summary>
    /// Register action-specific middlewares applied only for actions implementing <typeparamref name="TActionMarker"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenAction<TActionMarker>(this IMiddlewareRegistrator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.UseWhenAction(typeof(TActionMarker), subMiddlewares);
    }

    /// <summary>
    /// Register action-specific middlewares applied only for actions assignable to <paramref name="action"/> - the action
    /// type itself, or a marker interface or base class the actions share.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenAction(this IMiddlewareRegistrator configurator, Type action,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.UseWhen(a => action.IsAssignableFrom(a.GetType()), subMiddlewares);
    }

    #endregion

    #region UseWhenActions

    /// <summary>
    /// Register action-specific middlewares applied only for actions implementing <typeparamref name="TActionMarker1"/> or
    /// <typeparamref name="TActionMarker2"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenActions<TActionMarker1, TActionMarker2>(this IMiddlewareRegistrator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.UseWhenActions([typeof(TActionMarker1), typeof(TActionMarker2)], subMiddlewares);
    }

    /// <summary>
    /// Register action-specific middlewares applied only for actions implementing any of <typeparamref name="TActionMarker1"/>,
    /// <typeparamref name="TActionMarker2"/> or <typeparamref name="TActionMarker3"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenActions<TActionMarker1, TActionMarker2, TActionMarker3>(this IMiddlewareRegistrator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.UseWhenActions([typeof(TActionMarker1), typeof(TActionMarker2), typeof(TActionMarker3)], subMiddlewares);
    }

    /// <summary>
    /// Register action-specific middlewares applied only for actions assignable to any of <paramref name="actionMarkers"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenActions(this IMiddlewareRegistrator configurator, Type[] actionMarkers,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.UseWhen(action => actionMarkers.Any(t => t.IsAssignableFrom(action.GetType())), subMiddlewares);
    }

    #endregion

    #region UseWhenNotAction

    /// <summary>
    /// Register <typeparamref name="TMiddleware"/> applied only for actions not implementing <typeparamref name="TActionMarker"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenNotAction<TActionMarker, TMiddleware>(this IMiddlewareRegistrator configurator)
        where TMiddleware : IMediatorMiddleware
    {
        return configurator.UseWhenNotAction<TActionMarker>(m => m.Use<TMiddleware>());
    }

    /// <summary>
    /// Register action-specific middlewares applied only for actions not-implementing <typeparamref name="TActionMarker"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenNotAction<TActionMarker>(this IMiddlewareRegistrator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.UseWhenNotAction(typeof(TActionMarker), subMiddlewares);
    }

    /// <summary>
    /// Register action-specific middlewares applied only for actions not assignable to <paramref name="action"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseWhenNotAction(this IMiddlewareRegistrator configurator, Type action,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.UseWhen(a => action.IsAssignableFrom(a.GetType()) == false, subMiddlewares);
    }

    #endregion

    /// <summary>
    /// Register <see cref="HandlerExecutionMiddleware"/>, which runs the action's handlers and never calls the rest of the
    /// pipeline.
    /// </summary>
    /// <remarks>
    /// The mediator always ends the pipeline by running the handlers through the registered
    /// <see cref="IExecutionMiddleware"/>, so this is needed only to run handlers earlier - typically inside a
    /// <c>UseWhen*</c> branch, for actions that have to be handled locally on an HTTP client whose execution middleware
    /// would otherwise send them to the server.
    /// </remarks>
    public static IMiddlewareRegistrator UseHandlerExecution(this IMiddlewareRegistrator config)
    {
        return config.Use<HandlerExecutionMiddleware>();
    }

    /// <summary>
    /// Track actions processed by middleware through exposed events <see cref="ActionEventsMiddleware.ActionStarted"/>, <see cref="ActionEventsMiddleware.ProcessingStarted"/>, <see cref="ActionEventsMiddleware.ProcessingCompleted"/> and <see cref="ActionEventsMiddleware.ActionCompleted"/>
    /// </summary>
    /// <remarks>
    /// Registered as a singleton, so the events cover every action of the application - subscribe by injecting
    /// <see cref="ActionEventsMiddleware"/>. Typically used on a client to drive a global loading indicator.
    /// </remarks>
    public static IMiddlewareRegistrator UseActionEvents(this IMiddlewareRegistrator config)
    {
        return config.Use<ActionEventsMiddleware>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Register <see cref="NotificationReceiverMiddleware"/>, which passes the notifications in the results of every
    /// action to the subscribers of <see cref="INotificationReceiver.NotificationReceived"/>.
    /// </summary>
    public static IMiddlewareRegistrator UseNotificationReceiver(this IMiddlewareRegistrator config)
    {
        return config.Use<NotificationReceiverMiddleware>();
    }

    /// <summary>
    /// Register <see cref="AuthorizationMiddleware"/>, which evaluates the authorization policies of the action and its
    /// handlers and throws <see cref="AuthorizationException"/> unless they allow access.
    /// </summary>
    /// <remarks>
    /// Registered as a singleton. Place it before the middlewares that should not run for an unauthorized call.
    /// </remarks>
    public static IMiddlewareRegistrator UseAuthorization(this IMiddlewareRegistrator config)
    {
        return config.Use<AuthorizationMiddleware>(ServiceLifetime.Singleton);
    }

    /// <summary>
    /// Register <see cref="DirectCallProtectionMiddleware"/>, which fails an action called as the outermost mediator call
    /// with <see cref="MediatorException.CreateForForbidenDirectCall"/> - for actions that may only be called from
    /// another handler or middleware.
    /// </summary>
    /// <remarks>
    /// Usually registered inside a <c>UseWhen*</c> branch for the protected actions only.
    /// </remarks>
    public static IMiddlewareRegistrator UseDirectCallProtection(this IMiddlewareRegistrator config)
    {
        return config.Use<DirectCallProtectionMiddleware>();
    }
}