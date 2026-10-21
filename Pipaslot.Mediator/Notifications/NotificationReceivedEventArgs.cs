using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Notifications;

/// <summary>
/// Data of <see cref="INotificationReceiver.NotificationReceived"/>.
/// </summary>
public class NotificationReceivedEventArgs(IReadOnlyCollection<Notification> notifications) : EventArgs
{
    /// <summary>
    /// Notifications delivered together, ordered by <see cref="Notification.Time"/>.
    /// </summary>
    public IReadOnlyCollection<Notification> Notifications { get; } = notifications;
}