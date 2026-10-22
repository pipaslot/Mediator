namespace Pipaslot.Mediator.Notifications;

/// <summary>
/// Shortcuts for adding typed <see cref="Notification"/>s through <see cref="INotificationProvider"/>.
/// None of them changes <see cref="Middlewares.MediatorContext.Status"/>; to fail the action use
/// <see cref="Middlewares.MediatorContextExtensions.AddError(Middlewares.MediatorContext, string, bool)"/>.
/// </summary>
public static class NotificationProviderExtensions
{
    /// <summary>
    /// Adds a notification of the given <paramref name="type"/>.
    /// </summary>
    /// <param name="provider">Provider receiving the notification</param>
    /// <param name="content">Message text</param>
    /// <param name="source">Source name or title, usually the action name</param>
    /// <param name="type">Severity of the notification</param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void Add(this INotificationProvider provider, string content, string source, NotificationType type, bool stopPropagation = false)
    {
        provider.Add(new Notification { Content = content, Source = source, Type = type, StopPropagation = stopPropagation });
    }

    /// <summary>
    /// Adds a <see cref="NotificationType.Error"/> notification. It is reported to the caller as an error message but does not fail the action - use <see cref="Middlewares.MediatorContextExtensions.AddError(Middlewares.MediatorContext, string, bool)"/> for that.
    /// </summary>
    /// <param name="provider">Provider receiving the notification</param>
    /// <param name="content">Message text</param>
    /// <param name="source">Source name or title, usually the action name</param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddError(this INotificationProvider provider, string content, string source = "", bool stopPropagation = false)
    {
        provider.Add(content, source, NotificationType.Error, stopPropagation);
    }

    /// <summary>
    /// Adds a <see cref="NotificationType.Warning"/> notification.
    /// </summary>
    /// <param name="provider">Provider receiving the notification</param>
    /// <param name="content">Message text</param>
    /// <param name="source">Source name or title, usually the action name</param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddWarning(this INotificationProvider provider, string content, string source = "", bool stopPropagation = false)
    {
        provider.Add(content, source, NotificationType.Warning, stopPropagation);
    }

    /// <summary>
    /// Adds a <see cref="NotificationType.Information"/> notification.
    /// </summary>
    /// <param name="provider">Provider receiving the notification</param>
    /// <param name="content">Message text</param>
    /// <param name="source">Source name or title, usually the action name</param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddInformation(this INotificationProvider provider, string content, string source = "", bool stopPropagation = false)
    {
        provider.Add(content, source, NotificationType.Information, stopPropagation);
    }

    /// <summary>
    /// Adds a <see cref="NotificationType.Success"/> notification.
    /// </summary>
    /// <param name="provider">Provider receiving the notification</param>
    /// <param name="content">Message text</param>
    /// <param name="source">Source name or title, usually the action name</param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddSuccess(this INotificationProvider provider, string content, string source = "", bool stopPropagation = false)
    {
        provider.Add(content, source, NotificationType.Success, stopPropagation);
    }
}