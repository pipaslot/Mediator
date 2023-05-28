using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace Pipaslot.Mediator.Tests.ValidActions
{
    public class ScopedMessageHandler : IMessageHandler<ScopedMessage>, IScoped
    {
        public static HashSet<ScopedMessageHandler> Instances = new HashSet<ScopedMessageHandler>();
        public Task Handle(ScopedMessage action, CancellationToken cancellationToken)
        {
            Instances.Add(this);
            return Task.CompletedTask;
        }
    }

    public class ScopedMessage : IMessage
    {

    }
}
