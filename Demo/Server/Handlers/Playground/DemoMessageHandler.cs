using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground
{
    public class DemoMessageHandler : IMessageHandler<DemoMessage>
    {
        public Task Handle(DemoMessage request, CancellationToken cancellationToken)
        {
            if (request.Fail)
            {
                throw new Exception("Handler was not able to process MESSAGE sucessfully");
            }
            return Task.CompletedTask;
        }
    }
}
