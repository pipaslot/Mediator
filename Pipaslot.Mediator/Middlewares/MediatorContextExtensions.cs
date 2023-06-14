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
        public static void AddErrors(this MediatorContext context, IEnumerable<string> messages)
        {
            foreach (var message in messages)
            {
                context.AddError(message);
            }
        }

        /// <summary>
        /// Register processing error. Ignores duplicate entries.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message">The content</param>
        public static void AddError(this MediatorContext context, string message)
        {
            var notification = Notification.Error(message, context.Action);
            context.AddResult(notification);
        }

        /// <summary>
        /// Register processing error. Ignores duplicate entries.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="message">The content</param>
        /// <param name="source">Source name or title</param>
        public static void AddError(this MediatorContext context, string message, string source)
        {
            var notification = Notification.Error(message, source);
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
