using Pipaslot.Mediator;

namespace Demo.Shared.Auth
{
    public class ConditionalAuthenticationMessage : IMessage
    {
        public bool RunAsAdmin { get; set; }
        public string RequiredRole { get; set; }
    }
}
