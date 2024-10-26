using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground;

public class FailingMessageHandler : IMessageHandler<Failing.Message>
{
    public Task Handle(Failing.Message request, CancellationToken cancellationToken)
    {
        throw new Exception("Handler was not able to process MESSAGE sucessfully");
    }
}