using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator
{
    /// <summary>
    ///     Request / Message dispatched
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Execute action and wait for response data
        /// </summary>
        /// <typeparam name="TResult">Result object type returned from handler</typeparam>
        /// <param name="request">Object managing input parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Returns wrapper managing response state and data</returns>
        Task<IMediatorResponse<TResult>> Execute<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Send message without feedback expectation
        /// </summary>
        /// <param name="message">Object managing input parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Returns wrapper managing response state</returns>
        Task<IMediatorResponse> Dispatch(IMessage message, CancellationToken cancellationToken = default);
    }
}
