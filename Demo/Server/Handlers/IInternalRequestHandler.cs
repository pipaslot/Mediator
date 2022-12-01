using Demo.Shared;
using Pipaslot.Mediator.Abstractions;

namespace Demo.Server.Handlers
{
    public interface IInternalRequestHandler<in TRequest, TResponse> : IMediatorHandler<TRequest, TResponse>
        where TRequest : IInternalRequest<TResponse>
    {
    }
}
