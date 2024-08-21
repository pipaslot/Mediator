namespace Pipaslot.Mediator.Authorization.Formatting;

public record struct FormatedNode : INode
{
    public string Reason { get; }
    private FormatedNode(string reason) { Reason = reason; }
    
    public static FormatedNode Create(string reason) => new (reason.Trim());
}