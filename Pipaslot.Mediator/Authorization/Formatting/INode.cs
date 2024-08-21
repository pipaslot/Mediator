namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// Represents conversion from <see cref="Rule"/> and <see cref="RuleSet"/> to a structure of nodes passed to <see cref="INodeFormatter"/>.
/// The node tree is reduced to the relevant only depending on the <see cref="RuleSet.Operator"/>
/// </summary>
public interface INode
{
}