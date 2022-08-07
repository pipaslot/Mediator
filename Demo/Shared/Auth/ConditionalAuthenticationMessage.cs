using Pipaslot.Mediator;

namespace Demo.Shared.Auth
{
    public class ConditionalAuthenticationMessage : IAuthenticationFormMessage
    {
        public bool RequireAuthentication { get; set; }
        public string RequiredRole { get; set; }
    }
    public interface IAuthenticationFormMessage : IMessage
    {
        public bool RequireAuthentication { get; set; }
        public string RequiredRole { get; set; }
    }
}
