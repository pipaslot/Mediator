using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Pipaslot.Mediator.Tests.ValidActions
{
    public class InstanceCounterMessageHandler : IMessageHandler<InstanceCounterMessage>
    {
        public static HashSet<InstanceCounterMessageHandler> Instances = new HashSet<InstanceCounterMessageHandler>();
        public Task Handle(InstanceCounterMessage action, CancellationToken cancellationToken)
        {
            Instances.Add(this);
            return Task.CompletedTask;
        }
    }

    public class InstanceCounterMessage : IMessage
    {

    }
}
