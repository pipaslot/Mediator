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
    /// <exception cref="MediatorAmbiguousExceptionHandlerException">Two or more incomparable registrations match and neither is a class.</exception>
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

        // A candidate is rejected when another candidate's exception type is a proper subtype of it -
        // that other candidate is strictly more specific. What remains is the maximal (most-specific) set.
        var mostSpecific = candidates
            .Where(candidate => candidates.All(other =>
                ReferenceEquals(candidate, other)
                || !candidate.ExceptionType.IsAssignableFrom(other.ExceptionType)))
            .ToArray();

        if (mostSpecific.Length == 1)
        {
            return mostSpecific[0];
        }

        // Genuine tie between incomparable types (e.g. two unrelated interfaces): a class registration wins deterministically.
        var classCandidates = mostSpecific.Where(candidate => !candidate.ExceptionType.IsInterface).ToArray();
        if (classCandidates.Length == 1)
        {
            return classCandidates[0];
        }

        // classCandidates.Length > 1 cannot actually happen: two distinct *class* registrations that both match the
        // same concrete type must lie on that type's single linear base-class chain (C# classes have no multiple
        // inheritance), so one is always a strict ancestor of the other and would already have been eliminated by the
        // domination filter above. Only classCandidates.Length == 0 (every remaining candidate is an interface) reaches
        // this throw in practice - kept as a plain `!= 1` check rather than a dedicated `== 0` branch purely so the
        // method still fails safe (ambiguous, not silently wrong) if that invariant is ever violated.
        throw MediatorAmbiguousExceptionHandlerException.Create(concreteExceptionType, mostSpecific);
    }
}
