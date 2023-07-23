using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Notifications
{
    public class NotificationPropagationTests
    {
        private const string NotificationContent = "Hi, I am Elfo!";

        [Theory]
        [InlineData(ServiceType.NotificationProvider, 0, false, true, true)]//Stop propagation does not have effect when no nesting
        [InlineData(ServiceType.NotificationProvider, 0, false, false, true)]
        [InlineData(ServiceType.NotificationProvider, 1, false, true, false)]
        [InlineData(ServiceType.NotificationProvider, 1, false, false, true)]
        [InlineData(ServiceType.NotificationProvider, 2, false, true, false)]
        [InlineData(ServiceType.NotificationProvider, 2, false, false, true)]
        // stop propagation in set to false by middleware
        [InlineData(ServiceType.NotificationProvider, 0, true, true, true)]//Stop propagation does not have effect when no nesting
        [InlineData(ServiceType.NotificationProvider, 0, true, false, true)]
        [InlineData(ServiceType.NotificationProvider, 1, true, true, false)]
        [InlineData(ServiceType.NotificationProvider, 1, true, false, false)]
        [InlineData(ServiceType.NotificationProvider, 2, true, true, false)]
        [InlineData(ServiceType.NotificationProvider, 2, true, false, false)]

        [InlineData(ServiceType.ContextAccessor, 0, false, true, true)]//Stop propagation does not have effect when no nesting
        [InlineData(ServiceType.ContextAccessor, 0, false, false, true)]
        [InlineData(ServiceType.ContextAccessor, 1, false, true, false)]
        [InlineData(ServiceType.ContextAccessor, 1, false, false, true)]
        [InlineData(ServiceType.ContextAccessor, 2, false, true, false)]
        [InlineData(ServiceType.ContextAccessor, 2, false, false, true)]
        // stop propagation in set to false by middleware
        [InlineData(ServiceType.ContextAccessor, 0, true, true, true)]//Stop propagation does not have effect when no nesting
        [InlineData(ServiceType.ContextAccessor, 0, true, false, true)]
        [InlineData(ServiceType.ContextAccessor, 1, true, true, false)]
        [InlineData(ServiceType.ContextAccessor, 1, true, false, false)]
        [InlineData(ServiceType.ContextAccessor, 2, true, true, false)]
        [InlineData(ServiceType.ContextAccessor, 2, true, false, false)]

        [InlineData(ServiceType.Facade, 0, false, true, true)]//Stop propagation does not have effect when no nesting
        [InlineData(ServiceType.Facade, 0, false, false, true)]
        [InlineData(ServiceType.Facade, 1, false, true, false)]
        [InlineData(ServiceType.Facade, 1, false, false, true)]
        [InlineData(ServiceType.Facade, 2, false, true, false)]
        [InlineData(ServiceType.Facade, 2, false, false, true)]
        // stop propagation in set to false by middleware
        [InlineData(ServiceType.Facade, 0, true, true, true)]//Stop propagation does not have effect when no nesting
        [InlineData(ServiceType.Facade, 0, true, false, true)]
        [InlineData(ServiceType.Facade, 1, true, true, false)]
        [InlineData(ServiceType.Facade, 1, true, false, false)]
        [InlineData(ServiceType.Facade, 2, true, true, false)]
        [InlineData(ServiceType.Facade, 2, true, false, false)]
        public async Task TestPropagation(ServiceType serviceType, int depth, bool cancelPropagationByMiddleware, bool stopPropagation, bool shouldHaveNotification)
        {
            var sut = Factory.CreateConfiguredMediator(c => c.Use<StopPropagationMiddleware>());
            var res = await sut.Dispatch(new NotifyingAction(depth, stopPropagation, serviceType, cancelPropagationByMiddleware));
            var notifications = res.Results.Where(r => r is Notification).Cast<Notification>().ToList();
            if (shouldHaveNotification)
            {
                Assert.Single(notifications);
            }
            else
            {
                Assert.Empty(notifications);
            }
        }

        record NotifyingAction(int Depth, bool StopPropagation, ServiceType ServiceType, bool CancelPropagationByMiddleware) : IMediatorAction { }
        record NotifyingActionHandler : IMediatorHandler<NotifyingAction>
        {
            private readonly INotificationProvider _notificationProvider;
            private readonly IMediatorContextAccessor _contextAccessor;
            private readonly IMediatorFacade _facade;

            public NotifyingActionHandler(INotificationProvider notificationProvider, IMediatorContextAccessor contextAccessor, IMediatorFacade facade)
            {
                _notificationProvider = notificationProvider;
                _contextAccessor = contextAccessor;
                _facade = facade;
            }

            public async Task Handle(NotifyingAction action, CancellationToken cancellationToken)
            {
                if (action.Depth > 0)
                {
                    await _facade.Dispatch(action with { Depth = action.Depth - 1 }, cancellationToken);
                }
                else
                {
                    var notifiaction = new Notification
                    {
                        Content = NotificationContent,
                        Type = NotificationType.Success,
                        StopPropagation = action.StopPropagation
                    };
                    if (action.ServiceType == ServiceType.NotificationProvider)
                    {
                        _notificationProvider.Add(notifiaction);
                    }
                    else if (action.ServiceType == ServiceType.ContextAccessor)
                    {
                        _contextAccessor.Context.AddResult(notifiaction);
                    }
                    else if (action.ServiceType == ServiceType.Facade)
                    {
                        _facade.AddNotification(notifiaction);
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
            }
        }

        class StopPropagationMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                await next(context);
                if (context.Action is NotifyingAction action && action.CancelPropagationByMiddleware && context.ParentContexts.Any())
                {
                    var notifications = context.Results
                        .Where(r => r is Notification)
                        .Cast<Notification>();
                    foreach (var notification in notifications)
                    {
                        notification.StopPropagation = true;
                    }
                }
            }
        }
        public enum ServiceType
        {
            ContextAccessor,
            Facade,
            NotificationProvider
        }
    }
}
