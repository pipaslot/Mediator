using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.Notifications
{
    public class NotificationPropagationTests
    {
        private const string NotificationContent = "Hi, I am Elfo!";

        [Fact]
        public async Task GetNotificationFromRootHandler()
        {
            var sut = Factory.CreateMediator();
            var res = await sut.Dispatch(new NotifyingAction());
            var hasNotification = res.Results.Any(r => r is Notification n && n.Content == NotificationContent);
            Assert.True(hasNotification);
        }

        [Fact]
        public async Task GetNotificationFromNestedHandler()
        {
            var sut = Factory.CreateMediator();
            var res = await sut.Dispatch(new CallNotifyingAction());
            var hasNotification = res.Results.Any(r => r is Notification n && n.Content == NotificationContent);
            Assert.True(hasNotification);
        }

        record NotifyingAction : IMediatorAction { }
        record NotifyingActionHandler : IMediatorHandler<NotifyingAction>
        {
            private readonly INotificationProvider _notificationProvider;

            public NotifyingActionHandler(INotificationProvider notificationProvider)
            {
                _notificationProvider = notificationProvider;
            }

            public Task Handle(NotifyingAction action, CancellationToken cancellationToken)
            {
                _notificationProvider.AddSuccess(NotificationContent);
                return Task.CompletedTask;
            }
        }

        record CallNotifyingAction : IMediatorAction { }
        record CallNotifyingActionHandler : IMediatorHandler<CallNotifyingAction>
        {
            private readonly IMediator _mediator;

            public CallNotifyingActionHandler(IMediator mediator)
            {
                _mediator = mediator;
            }

            public Task Handle(CallNotifyingAction action, CancellationToken cancellationToken)
            {
                return _mediator.Dispatch(new NotifyingAction());
            }
        }
    }
}
