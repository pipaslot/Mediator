namespace Pipaslot.Mediator.Authorization.Formatting;

/// <summary>
/// A node whose reason is already rendered to text by <see cref="DefaultNodeFormatter"/>; the result of every
/// <c>Convert*</c> method and the value passed between them while the tree is formatted from the leaves to the root.
/// </summary>
/// <remarks>
/// The name is misspelled but is part of the public API, because <see cref="DefaultNodeFormatter"/>'s
/// <c>protected virtual</c> methods use it in their signatures. Return it from an override of
/// <see cref="DefaultNodeFormatter.ConvertRuleNode"/> or the other <c>Convert*</c> methods.
/// </remarks>
/// <param name="Reason">Rendered sentence. An empty or whitespace reason means "nothing to say" and the node is dropped
/// before siblings are joined.</param>
public readonly record struct FormatedNode(string Reason) : INode;