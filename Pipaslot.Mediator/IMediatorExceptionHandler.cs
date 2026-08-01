using System;
using System.Threading;
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
    /// Translates the caught exception into a client-safe message.
    /// </summary>
    Task<string> Handle(TException exception, CancellationToken cancellationToken);
}
