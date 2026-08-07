using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator;

/// <summary>
/// Action which does not return data. All derived types can have own specific pipelines and handlers.
/// Handled by <see cref="IMessageHandler{TMessage}"/> and dispatched via <see cref="IMediator.Dispatch"/>.
/// </summary>
/// <remarks>
/// One ready-made naming for <see cref="IMediatorAction"/>, which is where the contract is documented - notably that the
/// action goes to a single handler and that <see cref="IMediator.Dispatch"/> reports failure through the returned
/// <see cref="IMediatorResponse"/> instead of throwing. It adds nothing the root does not have: an action implementing
/// <c>ICommand</c> of your own behaves identically, and applications that prefer CQRS vocabulary commonly skip this type
/// entirely - see docs/wiki/9.1.-Custom-action-and-handler-types.md.
/// <para>
/// Use <see cref="IRequest{TResponse}"/> when the caller needs data back. Despite the name this is not an event
/// broadcast to subscribers; for a side channel that does not change the action's result, use
/// <see cref="Notifications.Notification"/>.
/// </para>
/// </remarks>
public interface IMessage : IMediatorAction;
