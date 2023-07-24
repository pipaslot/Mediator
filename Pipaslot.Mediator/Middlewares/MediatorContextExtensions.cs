using Pipaslot.Mediator.Notifications;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Middlewares
{
    public static class MediatorContextExtensions
    {
        /// <summary>
        /// Append result properties from context
        /// </summary>
        /// <param name="target">Target context</param>
        /// <param name="source">Source context</param>
        public static void Append(this MediatorContext target, MediatorContext source)
        {
            target.AddResults(source.Results);
        }

        /// <summary>
        /// Append result properties from response
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        public static void Append(this MediatorContext context, IMediatorResponse response)
        {
            context.AddResults(response.Results);
        }

        /// <summary>
        /// Register processing errors. Ignores duplicate entries.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="messages">The contents</param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddErrors(this MediatorContext context, IEnumerable<string> messages, bool stopPropagation = false)
        {
            foreach (var message in messages)
            {
                context.AddError(message, stopPropagation);
            }
        }

        /// <summary>
        /// Register processing error. Ignores duplicate entries.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message">The content</param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddError(this MediatorContext context, string message, bool stopPropagation = false)
        {
            var notification = Notification.Error(message, context.Action, stopPropagation);
            context.AddResult(notification);
        }

        /// <summary>
        /// Register processing error. Ignores duplicate entries.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message">The content</param>
        /// <param name="source">Source name or title</param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddError(this MediatorContext context, string message, string source, bool stopPropagation = false)
        {
            var notification = Notification.Error(message, source, stopPropagation);
            context.AddResult(notification);
        }

        /// <summary>
        /// Register processing results
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        public static void AddResults(this MediatorContext context, IEnumerable<object> result)
        {
            foreach (var res in result)
            {
                context.AddResult(res);
            }
        }
    }
}
