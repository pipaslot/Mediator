using System;

namespace Pipaslot.Mediator.Notifications
{
    public enum NotificationType
    {
        // TODO COnsider to get rid of this notification type from the app and let the responsibility of exception handling to consumer.
        // TODO Consider to add new type "Trace" for detailed notifications which may be interrested for troubleshooting by users
        /// <summary>
        /// Error produced by mediator during action processing in handler
        /// </summary>
        [Obsolete]
        ActionError = 0,
        /// <summary>
        /// Custom type provided by application or by middlewares
        /// </summary>
        Success = 1,
        /// <summary>
        /// Custom type provided by application or by middlewares
        /// </summary>
        Information = 2,
        /// <summary>
        /// Custom type provided by application or by middlewares
        /// </summary>
        Warning = 3,
        /// <summary>
        /// Custom type provided by application or by middlewares
        /// </summary>
        Error = 4
    }

    public static class NotificationTypeExtensions
    {
        public static bool IsError(this NotificationType type)
        {
            return type == NotificationType.Error || type == NotificationType.ActionError;
        }

        /// <summary>
        /// Conver ActionError to Error
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static NotificationType ToUnified(this NotificationType type)
        {
            if(type == NotificationType.ActionError)
            {
                return NotificationType.Error;
            }
            return type;
        }
    }
}
