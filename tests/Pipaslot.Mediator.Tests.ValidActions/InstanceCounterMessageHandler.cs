using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions;

public class InstanceCounterMessageHandler : IMessageHandler<InstanceCounterMessage>
{
    public static HashSet<InstanceCounterMessageHandler> Instances = new();

    public Task Handle(InstanceCounterMessage action, CancellationToken cancellationToken)
    {
        Instances.Add(this);
        return Task.CompletedTask;
    }
}

public class InstanceCounterMessage : IMessage
{
}