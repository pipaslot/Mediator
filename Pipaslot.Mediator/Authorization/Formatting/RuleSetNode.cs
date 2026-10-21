using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// A reduced <see cref="RuleSet"/>: the node <see cref="RuleSet.Reduce"/> returns (typed as <see cref="IRecursiveNode"/>)
/// for every rule set in the tree, including the root.
/// </summary>
/// <param name="RuleSet">The rule set this node was reduced from; the source of <see cref="Operator"/>.</param>
/// <param name="Children">Child nodes that decided <paramref name="Outcome"/>; empty when no child did, e.g. an
/// <see cref="Operator.Or"/> or <see cref="Operator.Add"/> set whose children were all <see cref="RuleOutcome.Ignored"/>.
/// An <see cref="Operator.And"/> set keeps ignored children as the denying ones.</param>
/// <param name="Outcome">Outcome of the set, computed from the children according to the operator.</param>
public readonly record struct RuleSetNode(RuleSet RuleSet, ICollection<IRecursiveNode> Children, RuleOutcome Outcome) : IRecursiveNode
{
    /// <summary>
    /// Always empty; a rule set has no text of its own, only its rule leaves do.
    /// </summary>
    public string Reason => string.Empty;

    /// <summary>
    /// The operator of <see cref="RuleSet"/>, used to join the children's reasons.
    /// </summary>
    public Operator Operator => RuleSet.Operator;
}