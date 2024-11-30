using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground;

public class CallingCustomInternalRequestMessageHandler(IMediator mediator) : IMessageHandler<CalingCustomInternalRequestMessage>
{
    public async Task Handle(CalingCustomInternalRequestMessage action, CancellationToken cancellationToken)
    {
        await mediator.ExecuteUnhandled(new CustomInternalRequest(), cancellationToken);
    }
}