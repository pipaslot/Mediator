using Pipaslot.Mediator.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

/// <summary>
/// Thrown when a caught exception type matches two or more registered <see cref="IMediatorExceptionHandler{TException}"/>
/// entries that are not comparable by specificity (e.g. two unrelated interfaces implemented by the same exception type).
/// This is a configuration bug and is treated as an unexpected failure by the Execute/Dispatch boundary.
/// </summary>
public class MediatorAmbiguousExceptionHandlerException(string message) : MediatorException(message)
{
    internal static MediatorAmbiguousExceptionHandlerException Create(Type concreteExceptionType, IReadOnlyCollection<ExceptionHandlerEntry> candidates)
    {
        // Sorted so the message is stable regardless of the backing dictionary's enumeration order - see the
        // determinism note in ExceptionHandlerCache.ResolveCore for why the routing decision itself never depends on it.
        var candidateTypes = string.Join(", ", candidates
            .Select(c => c.ExceptionType.ToString())
            .OrderBy(name => name, StringComparer.Ordinal));
        return new MediatorAmbiguousExceptionHandlerException(
            $"Multiple exception handlers were matched for exception type '{concreteExceptionType}' and none of them is more specific than the others. " +
            $"Candidates: [{candidateTypes}]. Register a handler for a common base type or interface, or remove one of the registrations, to resolve the ambiguity.");
    }
}
