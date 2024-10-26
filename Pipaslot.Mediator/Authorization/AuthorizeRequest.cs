using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Authorization;

[AnonymousPolicy]
public class AuthorizeRequest(IMediatorAction action) : IMediatorAction<AuthorizeRequestResponse>
{
    public IMediatorAction Action { get; } = action;
}