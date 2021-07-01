using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>
    /// Surround handler execution
    /// Implementations adds additional behavior and await the next delegate.
    /// </summary>
    public interface IMediatorMiddleware
    {
        /// <summary>
        /// Pipeline handler. Perform any additional behavior and await the <paramref name="next"/> delegate as necessary
        /// </summary>
        /// <param name="action">Incoming action request</param>
        /// <param name="response">Outgoing response</param>
        /// <param name="next">Awaitable delegate for the next middleware in the pipeline. Eventually this delegate represents the handler.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Awaitable task</returns>
        Task Invoke<TAction>(TAction action, MediatorResponse response, MiddlewareDelegate next, CancellationToken cancellationToken);
    }
}