using System;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator;

/// <summary>
/// Thrown when the handler completed successfully but did not produce a result of the expected type.
/// </summary>
public class MediatorMissingResultException(string message, MediatorContext? context) : MediatorExecutionException(message, context)
{
    internal static MediatorMissingResultException Create(Type type, MediatorContext context)
    {
        return new MediatorMissingResultException(
            $"Expected result type '{type}' was missing in result collection. Ensure that executed action has its handler.", context);
    }
}
