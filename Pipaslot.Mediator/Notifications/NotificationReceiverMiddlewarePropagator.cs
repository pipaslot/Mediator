using Microsoft.Extensions.DependencyInjection;
using System;

namespace Pipaslot.Mediator.Notifications;

public class NotificationReceiverMiddlewarePropagator(IServiceProvider serviceProvider) : INotificationProvider
{
    public void Add(Notification notification)
    {
        var messageReceiver = serviceProvider.GetService<NotificationReceiverMiddleware>();
        messageReceiver?.SendNotifications(notification); //TODO: log warning as the notification can be lost when the service is not available
    }
}