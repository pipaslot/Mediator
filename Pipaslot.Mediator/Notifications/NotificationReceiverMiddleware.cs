using Pipaslot.Mediator.Middlewares;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Notifications
{
    public class NotificationReceiverMiddleware : IMediatorMiddleware, INotificationReceiver
    {
        public event EventHandler<NotificationReceivedEventArgs>? NotificationReceived;

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            await next(context);

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
}
