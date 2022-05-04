using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Playground
{
    [AnonymousPolicy]
    public class MessageWithNotification : IMessage
    {
        public bool Fail { get; set; }
    }
}
