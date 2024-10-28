using Demo.Shared.Playground;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers.Playground;

public class MessageWithNotificationHandler(INotificationProvider notificationProvider) : IMessageHandler<MessageWithNotification>
{
    public Task Handle(MessageWithNotification action, CancellationToken cancellationToken)
    {
        if (action.Fail)
        {
            notificationProvider.AddWarning("Do you know that warnings can be propagated as well, event if exception is thrown?.", "Hi there");
            throw new Exception("Simulated handler failure.");
        }
        else
        {
            notificationProvider.AddSuccess("Message was accepted.", "Hi there");
        }

        return Task.CompletedTask;
    }
}