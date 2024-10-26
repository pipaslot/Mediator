using Demo.Shared.Playground;
using Pipaslot.Mediator.Abstractions;

namespace Demo.Server.Handlers.Playground;

public class CustomInternalRequestHandler : IMediatorHandler<CustomInternalRequest, bool>
{
    public Task<bool> Handle(CustomInternalRequest action, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }
}