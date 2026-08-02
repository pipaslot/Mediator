using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;

namespace Pipaslot.Mediator;

/// <summary>
/// Default <see cref="IMediatorExceptionContext"/> implementation. The boundary (<see cref="Mediator"/>) is its only
/// production caller; the public constructor exists so a handler can also be exercised as an integration-style test
/// against a real <see cref="MediatorContext"/> instead of only through a mock of the interface.
/// </summary>
public sealed class MediatorExceptionContext(Exception exception, MediatorContext context) : IMediatorExceptionContext
{
    public Exception Exception { get; } = exception;

    public MediatorContext Context { get; } = context;

    public CancellationToken CancellationToken => Context.CancellationToken;

    public bool IsHandled { get; private set; }

    public string? Message { get; private set; }

    public void SetHandled(string message)
    {
        IsHandled = true;
        Message = message;
    }

    public void SetHandledWithoutMessage()
    {
        IsHandled = true;
        Message = null;
    }

    public LogLevel LogLevel { get; private set; } = LogLevel.Warning;

    public void SetLogLevel(LogLevel level)
    {
        LogLevel = level;
    }
}
