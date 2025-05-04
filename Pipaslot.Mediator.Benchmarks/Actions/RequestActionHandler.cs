using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Benchmarks.Actions;

internal class RequestActionHandler : IMediatorHandler<RequestAction, string>
{
    public Task<string> Handle(RequestAction action, CancellationToken cancellationToken)
    {
        return Task.FromResult(action.Message);
    }
}