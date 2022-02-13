using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Notifications
{
    public class NotificationProviderMiddleware : IMediatorMiddleware, INotificationProvider
    {
        private ConcurrentBag<Notification> _notifications = new();

        public void Add(Notification notification)
        {
            if (notification is null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            _notifications.Add(notification);
        }

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            try
            {
                await next(context);
            }
            finally
            {
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
}
