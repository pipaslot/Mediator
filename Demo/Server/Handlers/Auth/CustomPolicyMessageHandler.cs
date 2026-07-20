using Demo.Shared.Auth;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers.Auth;

public class CustomPolicyMessageHandler(INotificationProvider notification)
    : IMessageHandler<CustomPolicyMessage>, IHandlerAuthorization<CustomPolicyMessage>
{
    public IPolicy Authorize(CustomPolicyMessage action)
    {
        return IdentityPolicy.Authenticated()
               & (Rule.UnavailableIf(!action.IsAvailable, "Sorry, not available!")
                  + Rule.DenyOrAllow(action.IsInvalid, "Model state does not allow to perform this operation.", "Go one!"));
    }

    public Task Handle(CustomPolicyMessage action, CancellationToken cancellationToken)
    {
        notification.AddSuccess("Handler was executed");
        return Task.CompletedTask;
    }
}