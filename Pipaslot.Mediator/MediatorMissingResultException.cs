using System;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator;

/// <summary>
/// Thrown when the handler completed successfully but did not produce a result of the expected type.
/// </summary>
/// <remarks>
/// The typical cause is a middleware that short-circuits the pipeline - it returns without awaiting <c>next</c>, so the
/// handler never runs, yet it leaves <see cref="Middlewares.MediatorContext.Status"/> at
/// <see cref="Middlewares.ExecutionStatus.Succeeded"/>. A middleware that stops an action has to fail it as well, via
/// <c>context.AddError(...)</c> or <c>context.AddException(...)</c>. The other cause is a middleware producing the result itself instead of delegating to a
/// handler and never calling <c>context.AddResult(...)</c> - passing <see cref="NullActionResult"/> there is how a
/// legitimately null result is reported, and that is exactly what the handler execution does for a handler returning null.
/// </remarks>
public class MediatorMissingResultException(string message, MediatorContext? context) : MediatorExecutionException(message, context)
{
    internal static MediatorMissingResultException Create(Type type, MediatorContext context)
    {
        return new MediatorMissingResultException(
            $"Expected result type '{type}' was missing in result collection. Ensure that executed action has its handler.", context);
    }
}
