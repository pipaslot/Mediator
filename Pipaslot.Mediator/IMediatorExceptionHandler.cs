using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
/// Translates an exception caught at the <see cref="IMediator.Execute{TResult}"/>/<see cref="IMediator.Dispatch"/> boundary
/// into a client-safe message. Register via <see cref="Configuration.IMediatorConfigurator.AddExceptionHandler{THandler}"/>.
/// A handler registered for a base exception type or interface is also used for its subtypes/implementations.
/// </summary>
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
