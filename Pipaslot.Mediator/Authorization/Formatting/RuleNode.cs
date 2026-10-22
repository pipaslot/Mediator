using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// The lowest level node without any children
/// </summary>
/// <param name="Rule">The evaluated rule; its <see cref="Rule.Outcome"/> is the outcome of the node.</param>
public readonly record struct RuleNode(Rule Rule) : IRecursiveNode
{
    /// <summary>
    /// Always <see cref="Operator.And"/>; a leaf has no children to combine.
    /// </summary>
    public Operator Operator => Operator.And;

    /// <summary>
    /// The outcome of <see cref="Rule"/>.
    /// </summary>
    public RuleOutcome Outcome => Rule.Outcome;

    /// <summary>
    /// Always empty.
    /// </summary>
    public ICollection<IRecursiveNode> Children => Array.Empty<IRecursiveNode>();

    /// <summary>
    /// Not available on an unformatted rule; convert the node to a <see cref="FormatedNode"/> and read its reason instead.
    /// </summary>
    /// <exception cref="InvalidOperationException">Always.</exception>
    public string Reason =>
        throw new InvalidOperationException(
            "Can not get reason from RuleNode because the node needs to be formated first (transformed to FormatedNode).");
}