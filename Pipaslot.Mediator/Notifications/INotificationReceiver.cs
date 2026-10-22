using System;

namespace Pipaslot.Mediator.Notifications;

/// <summary>
/// Receives a notifications from mediator communication
/// </summary>
public interface INotificationReceiver
{
    /// <summary>
    /// Raised with the notifications collected by an action once its pipeline finishes, ordered by
    /// <see cref="Notification.Time"/>, but only when the pipeline contains
    /// <see cref="Pipaslot.Mediator.MiddlewareRegistratorExtensions.UseNotificationReceiver(Pipaslot.Mediator.Configuration.IMiddlewareRegistrator)"/>. Raised immediately for a
    /// notification added outside of any action execution, or when <see cref="INotificationProvider"/> was registered with
    /// <c>addContextAccessor: false</c>. Not raised for an action that produced no notification.
    /// </summary>
    event EventHandler<NotificationReceivedEventArgs>? NotificationReceived;
}