using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Notifications;

public class NotificationReceivedEventArgs : EventArgs
{
    public IReadOnlyCollection<Notification> Notifications { get; }

    public NotificationReceivedEventArgs(IReadOnlyCollection<Notification> notifications)
    {
        Notifications = notifications;
    }
}