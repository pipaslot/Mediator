﻿using Pipaslot.Mediator.Middlewares;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Notifications
{
    public class NotificationReceiverMiddleware : IMediatorMiddleware, INotificationReceiver
    {
        public event EventHandler<NotificationReceivedEventArgs>? NotificationReceived;

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            await next(context);
            var errors = context.ErrorMessages
                .Select(message => new Notification
                {
                    Source = action?.GetType()?.ToString() ?? "",
                    Content = message,
                    Type = NotificationType.ActionError
                })
                .ToArray();

            var notifications = context.Results
                .Where(r => r is Notification)
                .Cast<Notification>()
                .ToArray();
            var ordered = errors.Concat(notifications).OrderBy(m => m.Time).ToArray();
            if (ordered.Any())
            {
                NotificationReceived?.Invoke(this, new NotificationReceivedEventArgs(ordered));
            }
        }
    }
}
