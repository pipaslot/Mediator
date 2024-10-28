using Demo.Shared.Playground;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers.Playground;

public class NestedNotificationMessageHandler(INotificationProvider notifications) : IMessageHandler<NestedNotificationMessage>
{
    public Task Handle(NestedNotificationMessage action, CancellationToken cancellationToken)
    {
        notifications.AddSuccess("Nested handler was executed");
        return Task.CompletedTask;
    }
}