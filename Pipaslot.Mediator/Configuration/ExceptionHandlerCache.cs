using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Configuration;

/// <summary>
/// Routes caught exception types to their registered <see cref="IMediatorExceptionHandler{TException}"/> executor.
/// Mirrors the startup/runtime two-tier caching used by <see cref="ReflectionCache"/>: registrations are collected
/// eagerly (startup), while resolution for a concrete runtime exception type is lazy and memoized on first occurrence,
/// because descendants of a registered exception type cannot be enumerated ahead of time.
/// </summary>
internal class ExceptionHandlerCache
{
    private readonly Dictionary<Type, ExceptionHandlerEntry> _entries = new();
    private readonly ConcurrentDictionary<Type, ExceptionHandlerEntry?> _resolutionCache = new();

    /// <summary>
    /// Number of times the candidate scan (<see cref="ResolveCore"/>) actually ran, i.e. cache-miss count.
    /// Test-only seam proving <see cref="Resolve"/> memoizes both positive and negative results instead of
    /// re-scanning routing entries on every call for the same concrete type.
    /// </summary>
    internal int ResolveScanCount { get; private set; }

    /// <summary>
    /// Registers a routing entry for an exact exception type.
    /// </summary>
    /// <exception cref="MediatorException">A handler for this exact exception type is already registered.</exception>
    internal void Add(Type exceptionType, Type executorType)
    {
        if (_entries.ContainsKey(exceptionType))
        {
            throw MediatorException.CreateForDuplicateExceptionHandler(exceptionType);
        }

        _entries[exceptionType] = new ExceptionHandlerEntry(exceptionType, executorType);
    }

    /// <summary>
    /// Resolves the most specific registered handler entry for a concrete runtime exception type, or null when none matches.
    /// The result - including a negative one - is memoized per concrete type so the candidate scan only ever runs once
    /// per distinct concrete exception type over the process lifetime.
    /// </summary>
    internal ExceptionHandlerEntry? Resolve(Type concreteExceptionType)
    {
        return _resolutionCache.GetOrAdd(concreteExceptionType, ResolveCore);
    }

    private ExceptionHandlerEntry? ResolveCore(Type concreteExceptionType)
    {
        ResolveScanCount++;
        var candidates = _entries.Values
            .Where(entry => entry.ExceptionType.IsAssignableFrom(concreteExceptionType))
            .ToArray();
        if (candidates.Length == 0)
        {
            return null;
        }

        if (candidates.Length == 1)
        {
            return candidates[0];
        }

        // IMediatorExceptionHandler<TException> constrains TException : Exception, so every registered exception
        // type is a class. Two or more candidates matching the same concrete type therefore always lie on that
        // type's single linear base-class chain (classes have no multiple inheritance) - one is always a strict
        // ancestor of the others, and picking the candidate no other candidate is assignable from always yields
        // exactly one, most-derived result.
        return candidates.Single(candidate => candidates.All(other =>
            ReferenceEquals(candidate, other)
            || !candidate.ExceptionType.IsAssignableFrom(other.ExceptionType)));
    }
}
