using Pipaslot.Mediator;
using Demo.Shared.Requests;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers
{
    public class MessageWithNotificationHandler : IMessageHandler<MessageWithNotification>
    {
        private readonly INotificationProvider _notificationProvider;

        public MessageWithNotificationHandler(INotificationProvider notificationProvider)
        {
            _notificationProvider = notificationProvider;
        }

        public Task Handle(MessageWithNotification action, CancellationToken cancellationToken)
        {
            _notificationProvider.AddSuccess("Message was accepted.", "Hi there");
            return Task.CompletedTask;
        }
    }
}
