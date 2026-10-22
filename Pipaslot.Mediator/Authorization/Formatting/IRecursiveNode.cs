using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// Node as conversion from Rules and RuleSets
/// </summary>
public interface IRecursiveNode : INode
{
    /// <summary>
    /// Nodes that decided <see cref="Outcome"/>; the children that did not contribute were removed by
    /// <see cref="RuleSet.Reduce"/>. Empty for a <see cref="RuleNode"/>.
    /// </summary>
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