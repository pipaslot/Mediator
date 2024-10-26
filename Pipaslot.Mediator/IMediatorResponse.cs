using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator
{
    public interface IMediatorResponse<TResult> : IMediatorResponse
    {
        /// <summary>
        /// Result from result set matching specified type
        /// </summary>
        TResult Result { get; }
    }

    public interface IMediatorResponse
    {
        bool Success { get; }
        /// <summary>
        /// Negated value of Success 
        /// </summary>
        bool Failure { get; }
        /// <summary>
        /// Results provided by middlewares and handlers
        /// </summary>
        object[] Results { get; }
    }

    public static class MediatorResponseExtensions
    {
        internal static TResult GetResult<TResult>(this IMediatorResponse response)
        {
            return (TResult)response.Results.FirstOrDefault(r => r is TResult)!;
        }
        
        internal static IEnumerable<Notification> GetNotifications(this ICollection<object> results)
        {
            return results
                .Where(r => r is Notification)
                .Cast<Notification>();
        }

        internal static IEnumerable<string> GetErrorMessages(this IEnumerable<Notification> notifications)
        {
            return notifications
               .Where(n => n.Type.IsError())
               .Select(n => n.Content);
        }

        public static string JoinErrorMessages(this IEnumerable<string> errors)
        {
            return string.Join(";", errors);
        }

        public static IEnumerable<Notification> GetNotifications(this IMediatorResponse response)
        {
            return response.Results
                .GetNotifications();
        }

        public static IEnumerable<string> GetErrorMessages(this IMediatorResponse response)
        {
            return response.Results
                .GetNotifications()
                .GetErrorMessages();
        }

        public static string GetErrorMessage(this IMediatorResponse response)
        {
            return response.Results
                .GetNotifications()
                .GetErrorMessages()
                .JoinErrorMessages();
        }
    }
}
