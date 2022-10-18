using Demo.Shared.Auth;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Auth
{
    public class IdenitityStaticAuthorizaMessageHander : IMessageHandler<IdenitityStaticAuthorizationMessage>
    {
        public Task Handle(IdenitityStaticAuthorizationMessage action, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
