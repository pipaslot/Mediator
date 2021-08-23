using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator
{
    /// <summary>Defines a default message handler which does not returns data</summary>
    /// <typeparam name="TMessage">The type of event being handled</typeparam>
    public interface IMessageHandler<in TMessage> : IMediatorHandler<TMessage> where TMessage : IMessage
    {
    }
}
