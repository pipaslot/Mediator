using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Notifications;

public class NotificationReceivedEventArgs(IReadOnlyCollection<Notification> notifications) : EventArgs
{
    public IReadOnlyCollection<Notification> Notifications { get; } = notifications;
}