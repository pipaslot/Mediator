using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Benchmarks.Actions;

internal class MessageActionHandler : IMediatorHandler<MessageAction>
{
    public Task Handle(MessageAction action, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}