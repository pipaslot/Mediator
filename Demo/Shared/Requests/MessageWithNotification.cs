using Pipaslot.Mediator;

namespace Demo.Shared.Requests
{
    public class MessageWithNotification : IMessage
    {
        public bool Fail { get; set; }
    }
}
