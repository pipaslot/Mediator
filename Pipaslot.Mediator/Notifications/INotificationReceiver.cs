using System;

namespace Pipaslot.Mediator.Notifications
{
    /// <summary>
    /// Receives a notifications from mediator communication
    /// </summary>
    public interface INotificationReceiver
    {
        event EventHandler<NotificationReceivedEventArgs>? NotificationReceived;
    }
}
