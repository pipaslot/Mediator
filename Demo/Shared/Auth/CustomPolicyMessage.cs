using Pipaslot.Mediator;

namespace Demo.Shared.Auth;

public class CustomPolicyMessage : IMessage
{
    public bool IsInvalid { get; set; }
    public bool IsAvailable { get; set; } = true;
}