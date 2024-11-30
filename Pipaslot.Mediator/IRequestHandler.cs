using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator;

/// <summary>Defines a default request handler</summary>
/// <typeparam name="TRequest">The type of request being handled</typeparam>
/// <typeparam name="TResponse">The type of response from the handler</typeparam>
public interface IRequestHandler<in TRequest, TResponse> : IMediatorHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>;