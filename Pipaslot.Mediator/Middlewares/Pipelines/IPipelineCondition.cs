using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Middlewares.Pipelines;

/// <summary>
/// Condition evaluated for pipeline activation
/// </summary>
public interface IPipelineCondition
{
    bool Matches(IMediatorAction action);
}