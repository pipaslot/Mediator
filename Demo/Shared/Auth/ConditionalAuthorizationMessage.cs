using Pipaslot.Mediator;

namespace Demo.Shared.Auth
{
    public class ConditionalAuthorizationMessage : IMessage
    {
        public bool RequireAuthentication { get; set; }
        public string RequiredRole { get; set; } = string.Empty;
    }
}
