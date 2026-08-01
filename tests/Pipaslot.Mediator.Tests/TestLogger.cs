using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;

namespace Pipaslot.Mediator.Tests;

/// <summary>
/// Minimal in-memory <see cref="ILogger{T}"/> recording every call verbatim (level, exception, formatted message),
/// so boundary-logging assertions don't need a real logging provider or Moq's clunky extension-method mocking.
/// </summary>
internal class TestLogger<T> : ILogger<T>
{
    public ConcurrentQueue<LogEntry> Entries { get; } = new();

    public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Entries.Enqueue(new LogEntry(logLevel, exception, formatter(state, exception)));
    }

    internal readonly record struct LogEntry(LogLevel Level, Exception? Exception, string Message);

    private sealed class NullScope : IDisposable
    {
        public static readonly NullScope Instance = new();
        public void Dispose()
        {
        }
    }
}
