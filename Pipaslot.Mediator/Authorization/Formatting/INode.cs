namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// This interface represents only abstraction to bound the inherited node kinds together.
/// Represents conversion from <see cref="Rule"/> and <see cref="RuleSet"/> to a structure of nodes passed to <see cref="INodeFormatter"/>.
/// The node tree is reduced to the relevant only depending on the <see cref="RuleSet.Operator"/>
/// </summary>
public interface INode;