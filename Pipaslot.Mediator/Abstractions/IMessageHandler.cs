using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>Defines a default message handler which does not returns data</summary>
    /// <typeparam name="TMessage">The type of event being handled</typeparam>
    public interface IMessageHandler<in TMessage> where TMessage : IMessage
    {
        /// <summary>Handles an message</summary>
        /// <param name="message">The message</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task Handle(TMessage message, CancellationToken cancellationToken);
    }
}
