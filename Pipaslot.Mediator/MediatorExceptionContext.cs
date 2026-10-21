using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;

namespace Pipaslot.Mediator;

/// <summary>
/// Default <see cref="IMediatorExceptionContext"/> implementation. The boundary (<see cref="Mediator"/>) is its only
/// production caller; the public constructor exists so a handler reading or writing <see cref="Context"/> can be
/// exercised against a real <see cref="MediatorContext"/> - see <see cref="MediatorContext.Create"/> - instead of
/// only through a mock of the interface.
/// </summary>
public sealed class MediatorExceptionContext(Exception exception, MediatorContext context) : IMediatorExceptionContext
{
    /// <inheritdoc/>
    public Exception Exception { get; } = exception;

    /// <inheritdoc/>
    public MediatorContext Context { get; } = context;

    /// <inheritdoc/>
    public CancellationToken CancellationToken => Context.CancellationToken;

    /// <inheritdoc/>
    public bool IsHandled { get; private set; }

    /// <inheritdoc/>
    public string? Message { get; private set; }

    /// <inheritdoc/>
    public void SetHandled(string message)
    {
        IsHandled = true;
        Message = message;
    }

    /// <inheritdoc/>
    public void SetHandledWithoutMessage()
    {
        IsHandled = true;
        Message = null;
    }

    /// <inheritdoc/>
    public void SetNotHandled()
    {
        IsHandled = false;
        Message = null;
    }

    /// <inheritdoc/>
    public LogLevel LogLevel { get; private set; } = LogLevel.Warning;

    /// <inheritdoc/>
    public void SetLogLevel(LogLevel level)
    {
        LogLevel = level;
    }
}
