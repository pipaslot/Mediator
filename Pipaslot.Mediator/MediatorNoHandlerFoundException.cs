using System;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator;

/// <summary>
/// Thrown when no handler was found for the executed action.
/// </summary>
/// <remarks>
/// Almost always a registration problem rather than a runtime one: the handler's assembly was never passed to
/// <c>AddHandlersFromAssemblyOf&lt;T&gt;</c>, or the handler implements the interface for a different action type than the
/// one dispatched. An action deliberately served by a middleware rather than a handler has to have that middleware
/// complete the execution without delegating further - <see cref="Abstractions.NoHandlerAttribute"/> only silences the
/// startup existence check, it does not affect this runtime failure. On the client side of the HTTP transport, the handler is expected on the
/// server - the client resolves an <see cref="Middlewares.IExecutionMiddleware"/>, not a handler.
/// </remarks>
public class MediatorNoHandlerFoundException(string message, MediatorContext? context) : MediatorExecutionException(message, context)
{
    internal static MediatorNoHandlerFoundException Create(Type? type, MediatorContext? context = null)
    {
        return new MediatorNoHandlerFoundException("No handler was found for " + type, context);
    }
}
