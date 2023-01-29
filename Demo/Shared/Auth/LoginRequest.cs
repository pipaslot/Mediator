using Demo.Shared.Auth.Dtos;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Auth
{
    [AnonymousPolicy]
    public class LoginRequest : IRequest<LoginRequestResult>
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

    }
}
