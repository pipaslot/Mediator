using Pipaslot.Mediator;
using Sample.Shared.Requests;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Server.Handlers
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
