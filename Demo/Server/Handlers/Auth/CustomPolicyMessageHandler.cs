using Demo.Shared.Auth;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Notifications;
using System.Diagnostics;

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
            var policy = IdentityPolicy.Authenticated();
            if (action.IsInvalid)
            {
                return policy.And(new RuleSet("Model state does not allow to perform this operation."));
            }
            return policy;
        }

        public Task Handle(CustomPolicyMessage action, CancellationToken cancellationToken)
        {
            _notification.AddSuccess("Handler was executed");
            return Task.CompletedTask;
        }
    }
}
