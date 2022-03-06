using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;

namespace Pipaslot.Mediator
{
    public static class MiddlewareRegistratorExtensions
    {
        /// <summary>
        /// Register action-specific middlewares applied only for actions implementing TActionMarker.
        /// </summary>
        public static IMiddlewareRegistrator MapWhen<TActionMarker>(this IMiddlewareRegistrator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.MapWhen(action => typeof(TActionMarker).IsAssignableFrom(action.GetType()), subMiddlewares);
        }

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
                services.TryAddScoped<INotificationReceiver>(s => s.GetService<NotificationReceiverMiddleware>());
            });
        }
    }
}
