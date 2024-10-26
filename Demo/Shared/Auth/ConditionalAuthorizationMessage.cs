using Pipaslot.Mediator;

namespace Demo.Shared.Auth;

public record ConditionalAuthorizationMessage : IMessage
{
    public bool RequireAuthentication { get; set; }
    public string RequiredRole { get; set; } = string.Empty;
}