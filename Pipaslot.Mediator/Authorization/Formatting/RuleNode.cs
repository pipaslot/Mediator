using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// The lowest level node without any children
/// </summary>
/// <param name="Rule"></param>
public readonly record struct RuleNode(Rule Rule) : IRecursiveNode
{
    public Operator Operator => Operator.And;
    public RuleOutcome Outcome => Rule.Outcome;
    public ICollection<IRecursiveNode> Children => Array.Empty<IRecursiveNode>();

    public string Reason =>
        throw new InvalidOperationException(
            "Can not get reason from RuleNode because the node needs to be formated first (transformed to FormatedNode).");
}