using Demo.Shared.Playground;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers.Playground
{
    public class NestedNotificationMessageHandler : IMessageHandler<NestedNotificationMessage>
    {
        private readonly INotificationProvider _notifications;

        public NestedNotificationMessageHandler(INotificationProvider notifications)
        {
            _notifications = notifications;
        }

        public Task Handle(NestedNotificationMessage action, CancellationToken cancellationToken)
        {
            _notifications.AddSuccess("Nested handler was executed");
            return Task.CompletedTask;
        }
    }
}
