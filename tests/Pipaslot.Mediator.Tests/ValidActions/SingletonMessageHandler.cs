using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions;

public class SingletonMessageHandler : IMessageHandler<SingletonMessage>, ISingleton
{
    public static readonly HashSet<SingletonMessageHandler> Instances = [];

    public Task Handle(SingletonMessage action, CancellationToken cancellationToken)
    {
        Instances.Add(this);
        return Task.CompletedTask;
    }
}

public class SingletonMessage : IMessage;