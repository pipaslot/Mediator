using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Authorization
{
    [AnonymousPolicy]
    public class AuthorizeRequest : IMediatorAction<AuthorizeRequestResponse>
    {
        public IMediatorAction Action { get; }

        public AuthorizeRequest(IMediatorAction action)
        {
            Action = action;
        }
    }
}
