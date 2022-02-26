﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;

namespace Pipaslot.Mediator
{
    public static class ConditionalPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Reduce action processing to only one at the same time for the same action type with the same properties.
        /// This is useful when you know that your application executes the same action multiple times but you want to reduce the server load. 
        /// IMPORTANT!: object method GetHashcode() is used for evaluating object similarities
        /// </summary>
        public static IConditionalPipelineConfigurator UseReduceDuplicateProcessing(this IConditionalPipelineConfigurator config)
        {
            return config.Use<ReduceDuplicateProcessingMiddleware>();
        }

        /// <summary>
        /// Track actions processed by middleware through exposed events
        /// </summary>
        public static IConditionalPipelineConfigurator UseActionEvents(this IConditionalPipelineConfigurator config)
        {
            return config.Use<ActionEventsMiddleware>(ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Middleware listening for error messages and <see cref="Notification"/> in action results which are exposed via event handler <see cref="INotificationReceiver.NotificationReceived"/>
        /// </summary>
        public static IConditionalPipelineConfigurator UseNotificationReceiver(this IConditionalPipelineConfigurator config)
        {
            return config.Use<NotificationReceiverMiddleware>(services =>
            {
                services.TryAddScoped<INotificationReceiver>(s => s.GetService<NotificationReceiverMiddleware>());
            });
        }
    }
}
