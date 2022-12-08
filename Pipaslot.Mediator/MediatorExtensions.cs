﻿using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator
{
    public static class MediatorExtensions
    {
        /// <summary>
        /// Execute action and return result if operation succeded otherwise returns default value for the result type
        /// </summary>
        /// <typeparam name="TResult">Action result type</typeparam>
        /// <returns>Action result type of its defaut implementation</returns>
        public static async Task<TResult?> ExecuteOrDefault<TResult>(this IMediator mediator, IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            var response = await mediator.Execute(request, cancellationToken);
            return response.Success ? response.Result : default;
        }

        /// <summary>
        /// Execute action and return result if operation succeded otherwise returns new instance of expected result type via parameterless constructor
        /// </summary>
        /// <typeparam name="TResult">Action result type</typeparam>
        /// <returns>Action result type of its new implementation</returns>
        public static async Task<TResult> ExecuteOrNew<TResult>(this IMediator mediator, IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
            where TResult : new()
        {
            var response = await mediator.Execute(request, cancellationToken);
            return response.Success
                ? response.Result ?? new TResult()
                : new TResult();
        }

        /// <summary>
        /// Call authorize method on handler to get feedback if current is permited to execute the request
        /// </summary>
        public static async Task<bool> IsAuthorized(this IMediator mediator, IMediatorAction action, CancellationToken cancellationToken = default)
        {
            var response = await mediator.Execute(new IsAuthorizedRequest(action), cancellationToken);
            return response.Success && response.Result.IsAuthorized;
        }

        /// <summary>
        /// Call authorize method on handler to get feedback if current is permited to execute the request
        /// </summary>
        public static async Task<IsAuthorizedRequestResponse> Authorize(this IMediator mediator, IMediatorAction action, CancellationToken cancellationToken = default)
        {
            var response = await mediator.Execute(new IsAuthorizedRequest(action), cancellationToken);
            return response.Success
                ? response.Result
                : new IsAuthorizedRequestResponse
                {
                    IsAuthorized = false
                };
        }
    }
}
