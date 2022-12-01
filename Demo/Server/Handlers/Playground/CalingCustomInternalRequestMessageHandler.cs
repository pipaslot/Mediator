using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground
{
    public class CalingCustomInternalRequestMessageHandler : IMessageHandler<CalingCustomInternalRequestMessage>
    {
        private readonly IMediator _mediator;

        public CalingCustomInternalRequestMessageHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(CalingCustomInternalRequestMessage action, CancellationToken cancellationToken)
        {
            await _mediator.ExecuteUnhandled(new CustomInternalRequest());
        }
    }
}
