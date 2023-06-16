using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Linq;

namespace Pipaslot.Mediator
{
    public static class MiddlewareRegistratorExtensions
    {
        /// <summary>
        /// Register middlewares applied only for actions passing the condition.
        /// </summary>
        public static IMiddlewareRegistrator UseWhen<TMiddleware>(this IMiddlewareRegistrator configurator, Func<IMediatorAction, bool> condition)
             where TMiddleware : IMediatorMiddleware
        {
            return configurator.UseWhen(condition, m => m.Use<TMiddleware>());
        }

        #region UseWhenAction

        /// <inheritdoc cref="UseWhenAction"/>
        public static IMiddlewareRegistrator UseWhenAction<TActionMarker, TMiddleware>(this IMiddlewareRegistrator configurator)
             where TMiddleware : IMediatorMiddleware
        {
            return configurator.UseWhenAction<TActionMarker>(m => m.Use<TMiddleware>());
        }

        /// <inheritdoc cref="UseWhenAction"/>
        public static IMiddlewareRegistrator UseWhenAction<TActionMarker>(this IMiddlewareRegistrator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhenAction(typeof(TActionMarker), subMiddlewares);
        }

        /// <summary>
        /// Register action-specific middlewares applied only for actions implementing TActionMarker.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenAction(this IMiddlewareRegistrator configurator, Type action, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhen(a => action.IsAssignableFrom(a.GetType()), subMiddlewares);
        }

        #endregion

        #region UseWhenActions

        /// <inheritdoc cref="UseWhenActions"/>
        public static IMiddlewareRegistrator UseWhenActions<TActionMarker1, TActionMarker2>(this IMiddlewareRegistrator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhenActions(new[] { typeof(TActionMarker1), typeof(TActionMarker2) }, subMiddlewares);
        }

        /// <inheritdoc cref="UseWhenActions"/>
        public static IMiddlewareRegistrator UseWhenActions<TActionMarker1, TActionMarker2, TActionMarker3>(this IMiddlewareRegistrator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhenActions(new[] { typeof(TActionMarker1), typeof(TActionMarker2), typeof(TActionMarker3) }, subMiddlewares);
        }

        /// <summary>
        /// Register action-specific middlewares applied only for actions implementing any actionMarker type
        /// </summary>
        public static IMiddlewareRegistrator UseWhenActions(this IMiddlewareRegistrator configurator, Type[] actionMarkers, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhen(action => actionMarkers.Any(t => t.IsAssignableFrom(action.GetType())), subMiddlewares);
        }

        #endregion

        #region UseWhenNotAction

        /// <inheritdoc cref="UseWhenNotAction"/>
        public static IMiddlewareRegistrator UseWhenNotAction<TActionMarker, TMiddleware>(this IMiddlewareRegistrator configurator)
             where TMiddleware : IMediatorMiddleware
        {
            return configurator.UseWhenNotAction<TActionMarker>(m => m.Use<TMiddleware>());
        }

        /// <inheritdoc cref="UseWhenNotAction"/>
        public static IMiddlewareRegistrator UseWhenNotAction<TActionMarker>(this IMiddlewareRegistrator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhenNotAction(typeof(TActionMarker), subMiddlewares);
        }

        /// <summary>
        /// Register action-specific middlewares applied only for actions not-implementing TActionMarker.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenNotAction(this IMiddlewareRegistrator configurator, Type action, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhen(a => action.IsAssignableFrom(a.GetType()) == false, subMiddlewares);
        }

        #endregion

        /// <inheritdoc cref="HandlerExecutionMiddleware"/>
        public static IMiddlewareRegistrator UseHandlerExecution(this IMiddlewareRegistrator config)
        {
            return config.Use<HandlerExecutionMiddleware>();
        }

        /// <inheritdoc cref="ReduceDuplicateProcessingMiddleware"/>
        public static IMiddlewareRegistrator UseReduceDuplicateProcessing(this IMiddlewareRegistrator config)
        {
            return config.Use<ReduceDuplicateProcessingMiddleware>();
        }

        /// <summary>
        /// Track actions processed by middleware through exposed events <see cref="ActionEventsMiddleware.ActionStarted"/> <see cref="ActionEventsMiddleware.ProcessingStarted"/>, <see cref="ActionEventsMiddleware.ProcessingCompleted"/> and  <see cref="ActionEventsMiddleware.ActionCompleted"/>
        /// </summary>
        public static IMiddlewareRegistrator UseActionEvents(this IMiddlewareRegistrator config)
        {
            return config.Use<ActionEventsMiddleware>(ServiceLifetime.Singleton);
        }

        /// <inheritdoc cref="NotificationReceiverMiddleware"/>
        public static IMiddlewareRegistrator UseNotificationReceiver(this IMiddlewareRegistrator config)
        {
            return config.Use<NotificationReceiverMiddleware>(services =>
            {
                services.TryAddScoped<INotificationReceiver>(s => s.GetRequiredService<NotificationReceiverMiddleware>());
            });
        }

        /// <inheritdoc cref="AuthorizationMiddleware"/>
        public static IMiddlewareRegistrator UseAuthorization(this IMiddlewareRegistrator config)
        {
            return config.Use<AuthorizationMiddleware>(ServiceLifetime.Singleton);
        }

        /// <inheritdoc cref="DirectCallProtectionMiddleware"/>
        public static IMiddlewareRegistrator UseDirectCallProtection(this IMiddlewareRegistrator config)
        {
            return config.Use<DirectCallProtectionMiddleware>(ServiceLifetime.Singleton);
        }
    }
}
