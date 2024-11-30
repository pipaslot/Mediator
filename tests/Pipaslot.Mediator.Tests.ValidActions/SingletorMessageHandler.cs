using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions;

public class SingletorMessageHandler : IMessageHandler<SingletorMessage>, ISingleton
{
    public static HashSet<SingletorMessageHandler> Instances = new();

    public Task Handle(SingletorMessage action, CancellationToken cancellationToken)
    {
        Instances.Add(this);
        return Task.CompletedTask;
    }
}

public class SingletorMessage : IMessage;