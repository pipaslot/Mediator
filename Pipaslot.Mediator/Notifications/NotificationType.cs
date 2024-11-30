namespace Pipaslot.Mediator.Notifications;

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
    /// Custom type provided by application or by middlewares
    /// </summary>
    Error = 4
}

public static class NotificationTypeExtensions
{
    public static bool IsError(this NotificationType type)
    {
        return type == NotificationType.Error;
    }
}