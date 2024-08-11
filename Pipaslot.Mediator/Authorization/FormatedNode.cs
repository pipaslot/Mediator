using Pipaslot.Mediator.Authorization.Formatting;

namespace Pipaslot.Mediator.Authorization;

/// <summary>
/// It is simplified structure of <see cref="EvaluatedNode"/> without outcome to keep it immutable (Because the <see cref="IEvaluatedNodeFormatter"/> can not change the evaluated <see cref="RuleOutcome"/>).
/// </summary>
/// <param name="Kind">Node kind</param>
/// <param name="Reason">Formated user-friendly text</param>
public record struct FormatedNode(string Kind, string Reason);