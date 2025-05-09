using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Benchmarks.Actions;

internal class RequestActionHandler : IMediatorHandler<RequestAction, RequestActionResult>
{
    public Task<RequestActionResult> Handle(RequestAction action, CancellationToken cancellationToken)
    {
        return Task.FromResult(new RequestActionResult(action.Message));
    }
}