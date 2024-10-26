using Demo.Shared.Playground;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers.Playground;

public class MessageWithNotificationHandler : IMessageHandler<MessageWithNotification>
{
    private readonly INotificationProvider _notificationProvider;

    public MessageWithNotificationHandler(INotificationProvider notificationProvider)
    {
        _notificationProvider = notificationProvider;
    }

    public Task Handle(MessageWithNotification action, CancellationToken cancellationToken)
    {
        if (action.Fail)
        {
            _notificationProvider.AddWarning("Do you know that warnings can be propagated as well, event if exception is thrown?.", "Hi there");
            throw new Exception("Simulated handler failure.");
        }
        else
        {
            _notificationProvider.AddSuccess("Message was accepted.", "Hi there");
        }

        return Task.CompletedTask;
    }
}