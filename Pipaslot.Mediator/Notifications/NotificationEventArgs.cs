using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Notifications
{
    public class NotificationEventArgs : EventArgs
    {
        public IReadOnlyCollection<Notification> Notifications { get; }

        public NotificationEventArgs(IReadOnlyCollection<Notification> notifications)
        {
            Notifications = notifications;
        }
    }
}
