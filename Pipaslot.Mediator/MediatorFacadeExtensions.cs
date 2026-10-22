using Pipaslot.Mediator.Notifications;

namespace Pipaslot.Mediator;

/// <summary>
/// Shorthands for <see cref="IMediatorFacade.AddNotification"/> that build the <see cref="Notification"/> from its parts.
/// </summary>
/// <remarks>
/// The same rules apply as for <see cref="IMediatorFacade.AddNotification"/>: an error notification is reported among the
/// error messages but does not fail the action.
/// </remarks>
public static class MediatorFacadeExtensions
{
    /// <summary>
    /// Adds a notification of the given <paramref name="type"/> to the action currently being executed.
    /// </summary>
    /// <param name="facade">The facade to add the notification through</param>
    /// <param name="content">Text of the notification, shown to the user</param>
    /// <param name="source"><inheritdoc cref="Notification.Source" path="/summary"/></param>
    /// <param name="type">Severity of the notification</param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddNotification(this IMediatorFacade facade, string content, string source, NotificationType type,
        bool stopPropagation = false)
    {
        facade.AddNotification(new Notification { Content = content, Source = source, Type = type, StopPropagation = stopPropagation });
    }

    /// <summary>
    /// Adds a notification of the given <paramref name="type"/> to the action currently being executed; the source is
    /// optional here.
    /// </summary>
    /// <param name="facade">The facade to add the notification through</param>
    /// <param name="content">Text of the notification, shown to the user</param>
    /// <param name="type">Severity of the notification</param>
    /// <param name="source"><inheritdoc cref="Notification.Source" path="/summary"/> Empty when null.</param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddNotification(this IMediatorFacade facade, string content, NotificationType type, string? source = null,
        bool stopPropagation = false)
    {
        facade.AddNotification(
            new Notification { Content = content, Source = source ?? string.Empty, Type = type, StopPropagation = stopPropagation });
    }

    /// <summary>
    /// Adds a <see cref="NotificationType.Error"/> notification to the action currently being executed. Does not fail the action - use <c>AddError</c> on the <see cref="Middlewares.MediatorContext"/> or throw for that.
    /// </summary>
    /// <param name="facade">The facade to add the notification through</param>
    /// <param name="content">Text of the notification, shown to the user</param>
    /// <param name="source"><inheritdoc cref="Notification.Source" path="/summary"/></param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddErrorNotification(this IMediatorFacade facade, string content, string source = "", bool stopPropagation = false)
    {
        facade.AddNotification(content, source, NotificationType.Error, stopPropagation);
    }

    /// <summary>
    /// Adds a <see cref="NotificationType.Warning"/> notification to the action currently being executed.
    /// </summary>
    /// <param name="facade">The facade to add the notification through</param>
    /// <param name="content">Text of the notification, shown to the user</param>
    /// <param name="source"><inheritdoc cref="Notification.Source" path="/summary"/></param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddWarningNotification(this IMediatorFacade facade, string content, string source = "", bool stopPropagation = false)
    {
        facade.AddNotification(content, source, NotificationType.Warning, stopPropagation);
    }

    /// <summary>
    /// Adds a <see cref="NotificationType.Information"/> notification to the action currently being executed.
    /// </summary>
    /// <param name="facade">The facade to add the notification through</param>
    /// <param name="content">Text of the notification, shown to the user</param>
    /// <param name="source"><inheritdoc cref="Notification.Source" path="/summary"/></param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddInformationNotification(this IMediatorFacade facade, string content, string source = "", bool stopPropagation = false)
    {
        facade.AddNotification(content, source, NotificationType.Information, stopPropagation);
    }

    /// <summary>
    /// Adds a <see cref="NotificationType.Success"/> notification to the action currently being executed.
    /// </summary>
    /// <param name="facade">The facade to add the notification through</param>
    /// <param name="content">Text of the notification, shown to the user</param>
    /// <param name="source"><inheritdoc cref="Notification.Source" path="/summary"/></param>
    /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
    public static void AddSuccessNotification(this IMediatorFacade facade, string content, string source = "", bool stopPropagation = false)
    {
        facade.AddNotification(content, source, NotificationType.Success, stopPropagation);
    }
}