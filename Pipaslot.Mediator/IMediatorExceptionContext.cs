using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;

namespace Pipaslot.Mediator;

/// <summary>
/// What a handler is allowed to do with a caught exception. Mock or fake this in handler unit tests;
/// the boundary always passes the <see cref="MediatorExceptionContext"/> implementation.
/// </summary>
public interface IMediatorExceptionContext
{
    /// <summary>
    /// The caught exception. Same instance as the typed method parameter.
    /// </summary>
    Exception Exception { get; }

    /// <summary>
    /// The pipeline context of the failing action (Action, Depth/IsNested, Features, Results, ...).
    /// </summary>
    MediatorContext Context { get; }

    /// <summary>
    /// Same value as <see cref="MediatorContext.CancellationToken"/>, including any replacement made by an earlier
    /// middleware via <see cref="MediatorContext.SetCancellationToken"/>.
    /// </summary>
    CancellationToken CancellationToken { get; }

    /// <summary>
    /// False until <see cref="SetHandled"/> is called. False means "fall through to the safe-by-default fallback,
    /// exactly as if no handler had been registered for this exception type".
    /// </summary>
    bool IsHandled { get; }

    /// <summary>
    /// Message the boundary adds to <see cref="MediatorContext.Results"/> when <see cref="IsHandled"/> is true.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Marks the exception as handled, with <paramref name="message"/> reported to the client via
    /// <see cref="MediatorContext.Results"/>.
    /// </summary>
    void SetHandled(string message);
}
