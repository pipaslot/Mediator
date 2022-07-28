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
        /// Concatenated error messages occured durign processing 
        /// </summary>
        [Obsolete("Use GetErrorMessage() instead")]
        string ErrorMessage { get; }
        /// <summary>
        /// Error messages occured durign processing 
        /// </summary>
        [Obsolete("Use GetErrorMessages() instead")]
        string[] ErrorMessages { get; }
        /// <summary>
        /// Results provided by middlewares and handlers
        /// </summary>
        object[] Results { get; }
    }

    public static class MediatorResponseExtensions
    {
        public static IEnumerable<Notification> GetNotifications(this IMediatorResponse response)
        {
            return response.Results
                .Where(r => r is Notification)
                .Cast<Notification>();
        }

        public static IEnumerable<string> GetErrorMessages(this IMediatorResponse response)
        {
            return response.GetNotifications()
                .Where(n => n.Type.IsError())
                .Select(n => n.Content);
        }

        public static string GetErrorMessage(this IMediatorResponse response)
        {
            return string.Join(";", response.GetErrorMessages());
        }
    }
}
