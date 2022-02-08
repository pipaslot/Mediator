using Pipaslot.Mediator.Middlewares;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Notifications
{
    public class NotificationSenderMiddleware : IMediatorMiddleware
    {
        private ConcurrentBag<Notification> _notifications = new();

        public void Add(params Notification[] notifications)
        {
            foreach (var notification in notifications)
            {
                _notifications.Add(notification);
            }
        }

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            await next(context);
            if (_notifications.Any())
            {
                context.Results.AddRange(_notifications);
                while (!_notifications.IsEmpty)
                {
                    _notifications.TryTake(out var _);
                }
            }
        }
    }
}
