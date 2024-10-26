namespace Pipaslot.Mediator;

/// <summary>
/// Enables executing of multiple handlers in sequence defined by Order. Handler with lover Order has higher priority during execution.
/// </summary>
public interface ISequenceHandler
{
    int Order { get; }
}