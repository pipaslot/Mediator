using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>
    ///     Request / Message dispatched
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Execute action and wait for response data
        /// </summary>
        Task<IMediatorResponse<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Send message without feedback expectation
        /// </summary>
        Task<IMediatorResponse> Dispatch(IMessage message, CancellationToken cancellationToken = default);
    }
}
