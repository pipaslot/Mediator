using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground
{
    public class CallingNestedNotificationMessageHandler : IMessageHandler<CallingNestedNotificationMessage>
    {
        private readonly IMediatorFacade _mediator;

        public CallingNestedNotificationMessageHandler(IMediatorFacade mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(CallingNestedNotificationMessage action, CancellationToken cancellationToken)
        {
            _mediator.AddInformationNotification("Greetings from root action");
            await _mediator.Dispatch(new NestedNotificationMessage(), cancellationToken);
            _mediator.AddSuccessNotification("Root handler was executed");
        }
    }
}
