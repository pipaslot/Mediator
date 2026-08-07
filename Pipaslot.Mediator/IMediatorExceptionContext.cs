using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;

namespace Pipaslot.Mediator;

/// <summary>
/// What a handler is allowed to do with a caught exception. Mock or fake this in handler unit tests;
/// the boundary always passes the <see cref="MediatorExceptionContext"/> implementation.
/// </summary>
/// <remarks>
/// Passed to <see cref="IMediatorExceptionHandler{TException}.Handle"/> as the only way to influence the outcome - a
/// handler reports its decision by calling <see cref="SetHandled"/>/<see cref="SetHandledWithoutMessage"/>, it does not
/// return a value and it must not rethrow. Handling an exception never turns the action into a success: it only replaces
/// the generic client-facing message with a specific one, and optionally the log level via <see cref="SetLogLevel"/>.
/// <para>
/// The failing action's own pipeline state is reachable through <see cref="Context"/>, so a handler can branch on
/// <see cref="MediatorContext.Action"/> or <see cref="MediatorContext.IsNested"/> instead of registering separate handler
/// types. Writing to <see cref="MediatorContext.Results"/> directly is not the intended route - use
/// <see cref="SetHandled"/> so that the boundary keeps ownership of the client-facing message.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public Task Handle(DbUpdateException exception, IMediatorExceptionContext context)
/// {
///     if (exception.IsUniqueConstraintViolation())
///     {
///         context.SetHandled("This record already exists.");
///     }
///     // Returning without calling SetHandled declines - the generic message and the Error log entry apply.
///     return Task.CompletedTask;
/// }
/// </code>
/// </example>
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
    /// False until <see cref="SetHandled"/>/<see cref="SetHandledWithoutMessage"/> is called, or after a subsequent
    /// <see cref="SetNotHandled"/>. False means "fall through to the safe-by-default fallback, exactly as if no
    /// handler had been registered for this exception type" - the same outcome as returning without calling either
    /// <c>SetHandled*</c> method in the first place.
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
    /// Reverses a decision to handle the exception - clears <see cref="IsHandled"/> and <see cref="Message"/>, so the
    /// boundary falls through to the safe-by-default fallback as if this handler had declined from the start. Lets a
    /// base-class handler's <see cref="SetHandled(string)"/>/<see cref="SetHandledWithoutMessage"/> call be reversed
    /// by an overriding subclass that inspects the concrete exception instance and decides not to translate it after
    /// all - the realistic case being a handler registered for a base exception type that only translates some of its
    /// subtypes. Resolution asks the single most specific registered handler once; declining does not fall back to a
    /// less specific one.
    /// </summary>
    void SetNotHandled();

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
