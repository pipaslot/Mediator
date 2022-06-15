using Demo.Shared.Auth;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Server.Handlers.Auth
{
    public class ConditionalAuthenticationMessageHandler : IMessageHandler<ConditionalAuthenticationMessage>, IHandlerAuthorization<ConditionalAuthenticationMessage>
    {
        public IPolicy Authorize(ConditionalAuthenticationMessage action)
        {
            var policy = action.RequireAuthentication
                ? IdentityPolicy.Authenticated()
                : IdentityPolicy.Anonymous();
            if (!string.IsNullOrWhiteSpace(action.RequiredRole))
            {
                policy.HasRole(action.RequiredRole);
            }
            return policy;
        }

        public Task Handle(ConditionalAuthenticationMessage action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
