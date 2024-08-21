using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization.Formatting;

public interface IRecursiveNode : INode
{
    public ICollection<IRecursiveNode> Children { get; }
    
    public Operator Operator { get; }
    
    /// <summary>
    /// Outcome evaluated from the actual child nodes
    /// </summary>
    public RuleOutcome Outcome { get; }
}