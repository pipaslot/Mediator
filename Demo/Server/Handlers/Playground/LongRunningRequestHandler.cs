using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground
{
    public class LongRunningRequestHandler : IRequestHandler<LongRunningRequest, bool>
    {
        public async Task<bool> Handle(LongRunningRequest action, CancellationToken cancellationToken)
        {
            await Task.Delay(action.Seconds * 1000, cancellationToken);
            return true;
        }
    }
}
