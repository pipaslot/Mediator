namespace Pipaslot.Mediator;

/// <summary>
/// Enables executing of multiple handlers in sequence defined by Order. Handler with lower Order has higher priority during execution.
/// </summary>
/// <remarks>
/// Every handler of the action has to implement it - mixing it with <see cref="IConcurrentHandler"/> or with unmarked
/// handlers fails with <see cref="MediatorException.CreateForCanNotCombineHandlers{THandler}"/>. Use
/// <see cref="IConcurrentHandler"/> when the handlers do not depend on each other's side effects and may run at the same time.
/// </remarks>
public interface ISequenceHandler
{
    /// <summary>
    /// Position of this handler in the sequence; handlers run in ascending order. The order of handlers with equal values
    /// is not guaranteed.
    /// </summary>
    int Order { get; }
}