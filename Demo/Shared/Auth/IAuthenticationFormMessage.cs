using Pipaslot.Mediator;

namespace Demo.Shared.Auth
{
    public interface IAuthenticationFormMessage : IMessage
    {
        public bool RequireAuthentication { get; set; }
        public string RequiredRole { get; set; }
    }
}
