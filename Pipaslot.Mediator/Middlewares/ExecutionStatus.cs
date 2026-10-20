namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Outcome of one pipeline run, stored in <see cref="MediatorContext.Status"/> and deciding <see cref="IMediatorResponse.Success"/>.
/// </summary>
/// <remarks>
/// A middleware or handler normally does not set it directly: <see cref="MediatorContextExtensions.AddError(MediatorContext, string, bool)"/>
/// and <see cref="MediatorContext.AddException"/> set <see cref="Failed"/> together with the reason.
/// </remarks>
public enum ExecutionStatus
{
    /// <summary>
    /// Mediator call will returns a success.
    /// </summary>
    Succeeded = 0,

    /// <summary>
    /// Error was detected during processing. Mediator call will returns a failure.
    /// </summary>
    Failed = 1,

    /// <summary>
    /// No handler was found in the handler execution middleware when it was expected. Mediator call will returns a failure
    /// </summary>
    /// <remarks>
    /// Unlike <see cref="Failed"/>, the mediator raises a <see cref="MediatorNoHandlerFoundException"/> for this status and
    /// handles it as any other unhandled exception - it marks a configuration error, not a business failure.
    /// </remarks>
    NoHandlerFound = 2
}