using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
/// Translates an exception caught at the <see cref="IMediator.Execute{TResult}"/>/<see cref="IMediator.Dispatch"/> boundary
/// into a client-safe message. Register via <see cref="Configuration.IMediatorConfigurator.AddExceptionHandler{THandler}"/>.
/// A handler registered for a base exception type or interface is also used for its subtypes/implementations.
/// </summary>
/// <remarks>
/// This is the supported replacement for a catch-all middleware that wrapped <c>next(context)</c> in a try/catch to turn
/// exceptions into error messages - such a middleware now competes with the boundary's own safe-by-default handling
/// instead of complementing it. Translation happens only on the <see cref="IMediator.Execute{TResult}"/>/<see cref="IMediator.Dispatch"/>
/// path; <see cref="IMediator.ExecuteUnhandled{TResult}"/>/<see cref="IMediator.DispatchUnhandled"/> rethrow instead, and
/// never consult a handler.
/// <para>
/// Without a registered handler, an exception reaches the client as a generic message and the detail stays in the server
/// log - so a handler is what makes a domain failure explainable, not what makes it safe. Resolution picks the single most
/// specific registered handler for the thrown type; declining does not fall back to a less specific one. See
/// docs/wiki/6.2.-Exception-handling.md.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// public class NotFoundExceptionHandler : IMediatorExceptionHandler&lt;NotFoundException&gt;
/// {
///     public Task Handle(NotFoundException exception, IMediatorExceptionContext context)
///     {
///         context.SetHandled($"{exception.EntityName} was not found.");
///         context.SetLogLevel(LogLevel.Information);
///         return Task.CompletedTask;
///     }
/// }
///
/// services.AddMediator()
///     .AddExceptionHandler&lt;NotFoundExceptionHandler&gt;();
/// </code>
/// </example>
/// <typeparam name="TException">Exception type (or common base type/interface) this handler translates</typeparam>
public interface IMediatorExceptionHandler<in TException> where TException : Exception
{
    /// <summary>
    /// Translates the caught exception into a client-safe message by calling <see cref="IMediatorExceptionContext.SetHandled"/>
    /// (or <see cref="IMediatorExceptionContext.SetHandledWithoutMessage"/> for a handled failure with no message) on
    /// <paramref name="context"/>. Returning without calling either declines - the boundary falls back to its generic
    /// message, the same outcome as calling <see cref="IMediatorExceptionContext.SetNotHandled"/> before returning
    /// (e.g. a base-class handler's decision reversed by an override after inspecting the concrete instance).
    /// </summary>
    Task Handle(TException exception, IMediatorExceptionContext context);
}
