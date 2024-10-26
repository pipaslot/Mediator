using Demo.Shared.Auth;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Server.Handlers.Auth;

public class ConditionalAuthorizationMessageHandler : IMessageHandler<ConditionalAuthorizationMessage>,
    IHandlerAuthorization<ConditionalAuthorizationMessage>
{
    public IPolicy Authorize(ConditionalAuthorizationMessage action)
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

    public Task Handle(ConditionalAuthorizationMessage action, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}