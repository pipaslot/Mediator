using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Auth;

[AuthenticatedPolicy]
public record IdenitityStaticAuthorizationMessage : IMessage;