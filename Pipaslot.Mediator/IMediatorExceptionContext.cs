using Microsoft.Extensions.Logging;
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
    /// False until <see cref="SetHandled"/>/<see cref="SetHandledWithoutMessage"/> is called. False means "fall
    /// through to the safe-by-default fallback, exactly as if no handler had been registered for this exception type".
    /// </summary>
    bool IsHandled { get; }

    /// <summary>
    /// Message the boundary adds to <see cref="MediatorContext.Results"/> when <see cref="IsHandled"/> is true.
    /// Null means "handled, no message" - the boundary then adds nothing to <see cref="MediatorContext.Results"/>.
    /// </summary>
    string? Message { get; }

    /// <summary>
    /// Marks the exception as handled, with <paramref name="message"/> reported to the client via
    /// <see cref="MediatorContext.Results"/>.
    /// </summary>
    void SetHandled(string message);

    /// <summary>
    /// Marks the exception as handled with no client-facing message - <see cref="Message"/> stays null and the
    /// boundary adds nothing to <see cref="MediatorContext.Results"/>. The action still fails: only the client-facing
    /// message is optional, not the failure itself.
    /// </summary>
    void SetHandledWithoutMessage();

    /// <summary>
    /// Level of the boundary's own log entry for the original exception, recorded when <see cref="IsHandled"/> is
    /// true. <see cref="LogLevel.Warning"/> by default; <see cref="LogLevel.None"/> suppresses the entry entirely -
    /// for a handler that already logs the failure itself. Has no effect when the handler ends up not handling the
    /// exception, since that path owns its own Error entry.
    /// </summary>
    LogLevel LogLevel { get; }

    /// <summary>
    /// Overrides <see cref="LogLevel"/> for the boundary's own log entry.
    /// </summary>
    void SetLogLevel(LogLevel level);
}
