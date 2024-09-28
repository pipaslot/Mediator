namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// Service used for policy transformation into a string message.
/// Policies expanded to sets of rules. The RuleSets are then reduced (nodes not applied in the decision making are ignored) and converted into Nodes.
/// <see cref="INode"/> represents <see cref="Rule"/> or <see cref="RuleSet"/> which were evaluated and the final outcome was produced.
/// </summary>
public interface INodeFormatter
{
    /// <summary>
    /// Function formating the reduced nodes and convert the rules to user-friendly message (Reason why the operation was allowed, denied or unavailable).
    /// The implementation has to be a recursive function looping through all the child node
    /// </summary>
    public string Format(IRecursiveNode rootNode);
}