using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;

namespace Pipaslot.Mediator
{
    public static class ConditionalPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Middleware running all handlers concurrently. Not further middleware will be executed after this one.
        /// </summary>
        public static IConditionalPipelineConfigurator UseConcurrentMultiHandler(this IConditionalPipelineConfigurator config)
        {
            return config.Use<MultiHandlerConcurrentExecutionMiddleware>();
        }

        /// <summary>
        /// Pipeline running all handlers in sequence one by one. Not further middleware will be executed after this one.
        /// For order specification see <see cref="ISequenceHandler"/>
        /// </summary>
        public static IConditionalPipelineConfigurator UseSequenceMultiHandler(this IConditionalPipelineConfigurator config)
        {
            return config.Use<MultiHandlerSequenceExecutionMiddleware>();
        }

        /// <summary>
        /// Execute single handler
        /// </summary>
        public static IConditionalPipelineConfigurator UseSingleHandler(this IConditionalPipelineConfigurator config)
        {
            return config.Use<SingleHandlerExecutionMiddleware>();
        }

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
        /// Middleware listening for error messages and <see cref="Notification"/> in action results which are exposed via event handler <see cref="NotificationReceiverMiddleware.NotificationsHasChanged"/>
        /// </summary>
        public static IConditionalPipelineConfigurator UseNotificationReceiver(this IConditionalPipelineConfigurator config)
        {
            return config.Use<NotificationReceiverMiddleware>();
        }

        /// <summary>
        /// Middleware attaching <see cref="Notification"/> to action results when new notification was added viat <see cref="NotificationSenderMiddleware.Add(Notification[])"/>
        /// </summary>
        public static IConditionalPipelineConfigurator UseNotificationSender(this IConditionalPipelineConfigurator config)
        {
            return config.Use<NotificationSenderMiddleware>();
        }
    }
}
