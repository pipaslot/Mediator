using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground;

public class CallingNestedNotificationMessageHandler(IMediatorFacade mediator) : IMessageHandler<CallingNestedNotificationMessage>
{
    public async Task Handle(CallingNestedNotificationMessage action, CancellationToken cancellationToken)
    {
        mediator.AddInformationNotification("Greetings from root action");
        await mediator.Dispatch(new NestedNotificationMessage(), cancellationToken);
        mediator.AddSuccessNotification("Root handler was executed");
    }
}