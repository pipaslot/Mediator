namespace Pipaslot.Mediator.Notifications;

/// <summary>
/// Severity of a <see cref="Notification"/>.
/// </summary>
public enum NotificationType
{
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
    /// Error message. The only type the library interprets: it is returned by <see cref="MediatorResponseExtensions.GetErrorMessages(IMediatorResponse)"/>
    /// and used in the text of <see cref="MediatorExecutionException"/>. Having this type does not make the action fail.
    /// </summary>
    Error = 4
}

/// <summary>
/// Helpers for <see cref="NotificationType"/>.
/// </summary>
public static class NotificationTypeExtensions
{
    /// <summary>
    /// True for <see cref="NotificationType.Error"/>.
    /// </summary>
    public static bool IsError(this NotificationType type)
    {
        return type == NotificationType.Error;
    }
}