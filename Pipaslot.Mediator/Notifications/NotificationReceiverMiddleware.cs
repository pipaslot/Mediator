using Pipaslot.Mediator.Middlewares;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Notifications;

/// <summary>
/// Expose all notifications sent as action results via event handler <see cref="INotificationReceiver.NotificationReceived"/>. 
/// You can then inject the <see cref="INotificationReceiver"/> and process/show all notifications via your own notification component (push notifications).
/// </summary>
public class NotificationReceiverMiddleware : IMediatorMiddleware, INotificationReceiver
{
    /// <inheritdoc/>
    public event EventHandler<NotificationReceivedEventArgs>? NotificationReceived;

    /// <summary>
    /// Runs the rest of the pipeline, then raises <see cref="NotificationReceived"/> with all <see cref="Notification"/>s
    /// found in <see cref="MediatorContext.Results"/>, ordered by <see cref="Notification.Time"/>.
    /// </summary>
    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        await next(context).ConfigureAwait(false);

        var notifications = context.Results
            .Where(r => r is Notification)
            .Cast<Notification>()
            .OrderBy(m => m.Time)
            .ToArray();
        if (notifications.Any())
        {
            NotificationReceived?.Invoke(this, new NotificationReceivedEventArgs(notifications));
        }
    }

    internal void SendNotifications(params Notification[] notifications)
    {
        NotificationReceived?.Invoke(this, new NotificationReceivedEventArgs(notifications));
    }
}