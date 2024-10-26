namespace Pipaslot.Mediator.Middlewares;

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
    NoHandlerFound = 2
}