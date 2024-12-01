using System.Collections.Generic;

namespace Pipaslot.Mediator.Authorization.Formatting;

public readonly record struct RuleSetNode(RuleSet RuleSet, ICollection<IRecursiveNode> Children, RuleOutcome Outcome) : IRecursiveNode
{
    public string Reason => string.Empty;
    public Operator Operator => RuleSet.Operator;
}