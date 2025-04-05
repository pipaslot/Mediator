using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground;

public class CallingCustomInternalRequestMessageHandler(IMediator mediator) : IMessageHandler<CallingCustomInternalRequestMessage>
{
    public async Task Handle(CallingCustomInternalRequestMessage action, CancellationToken cancellationToken)
    {
        await mediator.DispatchUnhandled(new CustomInternalRequest(), cancellationToken);
}
}