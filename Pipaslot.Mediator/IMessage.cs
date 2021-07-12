using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Action which does not return data. All derived types can have own specific pipelines and handlers.
    /// </summary>
    public interface IMessage : IMediatorAction
    {
    }
}
