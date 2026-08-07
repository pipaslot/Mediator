using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator;

/// <summary>
/// Defines a default request handler returning data for a single <see cref="IRequest{TResponse}"/> type.
/// Register it with <c>AddHandlers</c>/<c>AddHandlersFromAssemblyOf&lt;T&gt;</c> on the configurator returned by <c>AddMediator</c>.
/// </summary>
/// <remarks>
/// One ready-made naming for <see cref="IMediatorHandler{TAction,TResult}"/>, which is where the contract is documented -
/// registration, the one-handler-per-action rule, and how to fail an action. Registration never sees this interface: it
/// registers the handler under the closed <see cref="IMediatorHandler{TAction,TResult}"/> underneath, so implementing
/// that root directly, or an <c>IQueryHandler&lt;TQuery,TResult&gt;</c> of your own, is equally valid - see
/// docs/wiki/9.1.-Custom-action-and-handler-types.md.
/// <para>
/// Use <see cref="IMessageHandler{TMessage}"/> for actions that return nothing.
/// </para>
/// </remarks>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IMediatorHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>;
