using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Authorization
{
    [AnonymousPolicy]
    public class IsAuthorizedRequest : IMediatorAction<IsAuthorizedRequestResponse>
    {
        public IMediatorAction Action { get; }

        public IsAuthorizedRequest(IMediatorAction action)
        {
            Action = action;
        }
    }
}
