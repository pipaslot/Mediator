namespace Pipaslot.Mediator.Notifications
{
    public enum NotificationType
    {
        /// <summary>
        /// Error produced by mediator during action processing in handler
        /// </summary>
        ActionError,
        /// <summary>
        /// Custom type provided by application or by middlewares
        /// </summary>
        Success,
        /// <summary>
        /// Custom type provided by application or by middlewares
        /// </summary>
        Information,
        /// <summary>
        /// Custom type provided by application or by middlewares
        /// </summary>
        Warning,
        /// <summary>
        /// Custom type provided by application or by middlewares
        /// </summary>
        Error
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
