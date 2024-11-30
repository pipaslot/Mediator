using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions;

public class ScopedMessageHandler : IMessageHandler<ScopedMessage>, IScoped
{
    public static HashSet<ScopedMessageHandler> Instances = new();

    public Task Handle(ScopedMessage action, CancellationToken cancellationToken)
    {
        Instances.Add(this);
        return Task.CompletedTask;
    }
}

public class ScopedMessage : IMessage;