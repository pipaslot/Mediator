using Demo.Shared.Auth;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Notifications;

namespace Demo.Server.Handlers.Auth
{
    public class CustomPolicyMessageHandler : IMessageHandler<CustomPolicyMessage>, IHandlerAuthorization<CustomPolicyMessage>
    {
        private readonly INotificationProvider _notification;

        public CustomPolicyMessageHandler(INotificationProvider notification)
        {
            _notification = notification;
        }

        public IPolicy Authorize(CustomPolicyMessage action)
        {
            return IdentityPolicy.Authenticated()
                & Rule.Unavailable(!action.IsAvailable)
                & Rule.Allow(!action.IsInvalid,"Model state does not allow to perform this operation.", "Go one!");
        }

        public Task Handle(CustomPolicyMessage action, CancellationToken cancellationToken)
        {
            _notification.AddSuccess("Handler was executed");
            return Task.CompletedTask;
        }
    }
}
