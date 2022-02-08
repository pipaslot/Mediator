namespace Pipaslot.Mediator.Notifications
{
    public enum NotificationType
    {
        /// <summary>
        /// Error produced by mediator during action processing
        /// </summary>
        ActionError,
        /// <summary>
        /// Custom type provided by application
        /// </summary>
        Success,
        /// <summary>
        /// Custom type provided by application
        /// </summary>
        Information,
        /// <summary>
        /// Custom type provided by application
        /// </summary>
        Warning,
        /// <summary>
        /// Custom type provided by application
        /// </summary>
        Error
    }
}
