using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator;

/// <summary>
/// Defines a default message handler which does not return data, for a single <see cref="IMessage"/> type.
/// Register it with <c>AddHandlers</c>/<c>AddHandlersFromAssemblyOf&lt;T&gt;</c> on the configurator returned by <c>AddMediator</c>.
/// </summary>
/// <remarks>
/// One ready-made naming for <see cref="IMediatorHandler{TAction}"/>, which is where the contract is documented -
/// registration, the one-handler-per-action rule, and how to fail an action. Registration never sees this interface: it
/// registers the handler under the closed <see cref="IMediatorHandler{TAction}"/> underneath, so implementing that root
/// directly, or an <c>ICommandHandler&lt;TCommand&gt;</c> of your own, is equally valid - see
/// docs/wiki/9.1.-Custom-action-and-handler-types.md.
/// <para>
/// Use <see cref="IRequestHandler{TRequest,TResponse}"/> when the caller needs data back.
/// </para>
/// </remarks>
/// <typeparam name="TMessage">The type of event being handled</typeparam>
public interface IMessageHandler<in TMessage> : IMediatorHandler<TMessage> where TMessage : IMessage;
