﻿using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator
{
    /// <summary>
    ///     Request / Message dispatched
    /// </summary>
    public interface IMediator
    {
        /// <summary>
        /// Execute action and wait for response data
        /// </summary>
        /// <typeparam name="TResult">Result object type returned from handler</typeparam>
        /// <param name="request">Object managing input parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="defaultFeatures">Features propagated to <see cref="MediatorContext.Features"/> to provide and customize configuration for middlewares when can not extend action object.</param>
        /// <returns>Returns wrapper managing response state and data</returns>
        Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default, IFeatureCollection? defaultFeatures = null);

        /// <summary>
        /// Send message without feedback expectation
        /// </summary>
        /// <param name="message">Object managing input parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="defaultFeatures">Features propagated to <see cref="MediatorContext.Features"/> to provide and customize configuration for middlewares when can not extend action object.</param>
        /// <returns>Returns wrapper managing response state</returns>
        Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default, IFeatureCollection? defaultFeatures = null);

        /// <summary>
        /// Execute action and wait for response data. Exception will be thrown if processing was unsuccessfull.
        /// </summary>
        /// <typeparam name="TResult">Result object type returned from handler</typeparam>
        /// <param name="request">Object managing input parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="defaultFeatures">Features propagated to <see cref="MediatorContext.Features"/> to provide and customize configuration for middlewares when can not extend action object.</param>
        /// <returns>Returns data from handler</returns>
        Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default, IFeatureCollection? defaultFeatures = null);

        /// <summary>
        /// Send message without feedback expectation. Exception will be thrown if processing was unsuccessfull.
        /// </summary>
        /// <param name="message">Object managing input parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <param name="defaultFeatures">Features propagated to <see cref="MediatorContext.Features"/> to provide and customize configuration for middlewares when can not extend action object.</param>
        /// <returns>Does not returns either data nor state</returns>
        Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default, IFeatureCollection? defaultFeatures = null);
    }
}
