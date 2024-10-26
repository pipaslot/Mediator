using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions
{
    public class NopMessage : IMessage
    {
    }

    public class NopMesageHandler : IMessageHandler<NopMessage>
    {
        public Task Handle(NopMessage action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}