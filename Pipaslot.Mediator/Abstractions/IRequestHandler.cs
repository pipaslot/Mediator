﻿using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>Defines a default request handler</summary>
    /// <typeparam name="TRequest">The type of request being handled</typeparam>
    /// <typeparam name="TResponse">The type of response from the handler</typeparam>
    public interface IRequestHandler<in TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        /// <summary>Handles a request</summary>
        /// <param name="request">The request</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Response from the request</returns>
        Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
    }
}