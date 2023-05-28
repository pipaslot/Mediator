﻿using Microsoft.Extensions.DependencyInjection;
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

        /// <summary>
        /// Register action-specific middlewares applied only for actions implementing TActionMarker.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenAction<TActionMarker, TMiddleware>(this IMiddlewareRegistrator configurator)
             where TMiddleware : IMediatorMiddleware
        {
            return configurator.UseWhenAction<TActionMarker>(m => m.Use<TMiddleware>());
        }

        /// <summary>
        /// Register action-specific middlewares applied only for actions implementing TActionMarker.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenAction<TActionMarker>(this IMiddlewareRegistrator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhen(action => typeof(TActionMarker).IsAssignableFrom(action.GetType()), subMiddlewares);
        }

        #endregion

        #region UseWhenActions

        /// <summary>
        /// Register action-specific middlewares applied only for actions implementing any actionMarker type.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenActions<TActionMarker1, TActionMarker2>(this IMiddlewareRegistrator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhenActions(new[] { typeof(TActionMarker1), typeof(TActionMarker2) }, subMiddlewares);
        }

        /// <summary>
        /// Register action-specific middlewares applied only for actions implementing any actionMarker type.
        /// </summary>
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

        /// <summary>
        /// Register action-specific middlewares applied only for actions not-implementing TActionMarker.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenNotAction<TActionMarker, TMiddleware>(this IMiddlewareRegistrator configurator)
             where TMiddleware : IMediatorMiddleware
        {
            return configurator.UseWhenNotAction<TActionMarker>(m => m.Use<TMiddleware>());
        }

        /// <summary>
        /// Register action-specific middlewares applied only for actions not-implementing TActionMarker.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenNotAction<TActionMarker>(this IMiddlewareRegistrator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.UseWhen(action => typeof(TActionMarker).IsAssignableFrom(action.GetType()) == false, subMiddlewares);
        }

        #endregion

        /// <summary>
        /// Execute handlers. No more middlewares will be executed.
        /// </summary>
        public static IMiddlewareRegistrator UseHandlerExecution(this IMiddlewareRegistrator config)
        {
            return config.Use<HandlerExecutionMiddleware>();
        }

        /// <summary>
        /// Reduce action processing to only one at the same time for the same action type with the same properties.
        /// This is useful when you know that your application executes the same action multiple times but you want to reduce the server load. 
        /// IMPORTANT!: object method GetHashcode() is used for evaluating object similarities
        /// </summary>
        public static IMiddlewareRegistrator UseReduceDuplicateProcessing(this IMiddlewareRegistrator config)
        {
            return config.Use<ReduceDuplicateProcessingMiddleware>();
        }

        /// <summary>
        /// Track actions processed by middleware through exposed events
        /// </summary>
        public static IMiddlewareRegistrator UseActionEvents(this IMiddlewareRegistrator config)
        {
            return config.Use<ActionEventsMiddleware>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Middleware listening for error messages and <see cref="Notification"/> in action results which are exposed via event handler <see cref="INotificationReceiver.NotificationReceived"/>
        /// </summary>
        public static IMiddlewareRegistrator UseNotificationReceiver(this IMiddlewareRegistrator config)
        {
            return config.Use<NotificationReceiverMiddleware>(services =>
            {
                services.TryAddScoped<INotificationReceiver>(s => s.GetRequiredService<NotificationReceiverMiddleware>());
            });
        }

        /// <summary>
        /// Register authorization middleware evaluating policies for actions and their handlers
        /// </summary>
        public static IMiddlewareRegistrator UseAuthorization(this IMiddlewareRegistrator config)
        {
            return config.Use<AuthorizationMiddleware>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Prevent direct calls for action which are not part of your application API. 
        /// Can be used as protection for queries placed in app demilitarized zone. Such a actions lacks authentication, authorization or different security checks.
        /// </summary>
        public static IMiddlewareRegistrator UseDirectCallProtection(this IMiddlewareRegistrator config)
        {
            return config.Use<DirectCallProtectionMiddleware>(ServiceLifetime.Singleton);
        }
    }
}
