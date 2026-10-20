using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Middlewares.Pipelines;

/// <summary>
/// Condition evaluated for pipeline activation
/// </summary>
public interface IPipelineCondition
{
    /// <summary>
    /// Returns true when the pipeline should handle <paramref name="action"/>. When more than one
    /// registered pipeline matches the same action, the dispatch fails with a <see cref="MediatorException"/>.
    /// </summary>
    bool Matches(IMediatorAction action);
}