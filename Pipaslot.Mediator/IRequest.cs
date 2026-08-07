using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator;

/// <summary>
/// Action which returns data. All derived types can have own specific pipelines and handlers.
/// Handled by <see cref="IRequestHandler{TRequest,TResponse}"/> and dispatched via <see cref="IMediator.Execute{TResult}"/>.
/// </summary>
/// <remarks>
/// One ready-made naming for <see cref="IMediatorAction{TResult}"/>, which is where the contract is documented - notably
/// that <see cref="IMediator.Execute{TResult}"/> hands back an <see cref="IMediatorResponse{TResult}"/> wrapper rather
/// than a bare <typeparamref name="TResponse"/>. It adds nothing the root does not have: the mediator treats an action
/// implementing <c>IQuery&lt;TResult&gt;</c> of your own exactly the same, and applications that prefer CQRS vocabulary
/// commonly skip this type entirely - see docs/wiki/9.1.-Custom-action-and-handler-types.md.
/// <para>
/// Use <see cref="IMessage"/> for an action which returns nothing.
/// </para>
/// </remarks>
/// <typeparam name="TResponse">Result data returned from handler execution</typeparam>
public interface IRequest<out TResponse> : IRequest, IMediatorAction<TResponse>;

/// <summary>
/// Marker interface for IRequest action type.
/// Use only for pipeline configuration to define middlewares applicable for this action type..
/// </summary>
/// <remarks>
/// Do not implement this one directly on an action - implement <see cref="IRequest{TResponse}"/>, which carries the result
/// type. This non-generic marker exists so that a pipeline can be registered for every request at once, for example
/// <c>AddPipelineForAction&lt;IRequest&gt;(p =&gt; p.Use&lt;MyMiddleware&gt;())</c>. A custom marker interface serves that
/// purpose just as well, which is the usual reason to define one.
/// </remarks>
public interface IRequest : IMediatorAction;
