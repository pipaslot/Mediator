namespace Pipaslot.Mediator;

/// <summary>
/// Enables executing of multiple handler at the same time.
/// </summary>
/// <remarks>
/// Every handler of the action has to implement it - mixing it with <see cref="ISequenceHandler"/> or with unmarked
/// handlers fails with <see cref="MediatorException.CreateForCanNotCombineHandlers{THandler}"/>. The mediator waits for
/// all handlers before it continues. Use <see cref="ISequenceHandler"/> when the handlers have to run in a defined order.
/// </remarks>
public interface IConcurrentHandler;