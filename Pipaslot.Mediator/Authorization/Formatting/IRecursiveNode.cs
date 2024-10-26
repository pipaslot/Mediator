using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// Node as conversion from Rules and RuleSets
/// </summary>
public interface IRecursiveNode : INode
{
    public ICollection<IRecursiveNode> Children { get; }

    /// <summary>
    /// Operator used for combining of the child nodes
    /// </summary>
    public Operator Operator { get; }

    /// <summary>
    /// Outcome evaluated from the actual child nodes
    /// </summary>
    public RuleOutcome Outcome { get; }
}