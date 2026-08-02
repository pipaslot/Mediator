using System;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator;

/// <summary>
/// Thrown when no handler was found for the executed action.
/// </summary>
public class MediatorNoHandlerFoundException(string message, MediatorContext? context) : MediatorExecutionException(message, context)
{
    internal static MediatorNoHandlerFoundException Create(Type? type, MediatorContext? context = null)
    {
        return new MediatorNoHandlerFoundException("No handler was found for " + type, context);
    }
}
