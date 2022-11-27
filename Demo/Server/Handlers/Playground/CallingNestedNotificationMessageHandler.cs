using Demo.Shared.Playground;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers.Playground
{
    public class CallingNestedNotificationMessageHandler : IMessageHandler<CallingNestedNotificationMessage>
    {
        private readonly IMediator _mediator;
        private readonly INotificationProvider _notifications;

        public CallingNestedNotificationMessageHandler(IMediator mediator, INotificationProvider notifications)
        {
            _mediator = mediator;
            _notifications = notifications;
        }

        public async Task Handle(CallingNestedNotificationMessage action, CancellationToken cancellationToken)
        {
            _notifications.AddInformation("Greetings from root action");
            await _mediator.Dispatch(new NestedNotificationMessage(), cancellationToken);
            _notifications.AddSuccess("Root handler was executed");
        }
    }
}
