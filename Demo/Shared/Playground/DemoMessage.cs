using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Playground
{
    [AnonymousPolicy]
    public class DemoMessage : IMessage
    {
        public bool Fail { get; set; }
    }
}
