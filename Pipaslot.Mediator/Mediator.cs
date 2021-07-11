﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Mediator which wrapps handler execution into pipelines
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly ServiceResolver _handlerResolver;

        public Mediator(ServiceResolver handlerResolver)
        {
            _handlerResolver = handlerResolver;
        }

        public async Task<IMediatorResponse> Dispatch(IMessage message, CancellationToken cancellationToken = default)
        {
            var pipeline = _handlerResolver.GetPipeline(message.GetType());
            static Task Seed(MediatorResponse response) => Task.CompletedTask;
            var response = new MediatorResponse();
            try
            {
                await pipeline
                    .Reverse()
                    .Aggregate((MiddlewareDelegate)Seed,
                        (next, middleware) => (MediatorResponse res) => middleware.Invoke(message, res, next, cancellationToken))(response);

                return response;
            }
            catch (Exception e)
            {
                return new MediatorResponse(e.Message);
            }
        }

        public async Task<IMediatorResponse<TResponse>> Execute<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _handlerResolver.GetPipeline(request.GetType());
            static Task Seed(MediatorResponse res) => Task.CompletedTask;
            var response = new MediatorResponse<TResponse>();
            try
            {
                await pipeline
                    .Reverse()
                    .Aggregate((MiddlewareDelegate)Seed,
                        (next, middleware) => (MediatorResponse res) => middleware.Invoke(request, res, next, cancellationToken))(response);

                return response;
            }
            catch (Exception e)
            {
                return new MediatorResponse<TResponse>(e.Message);
            }
        }
    }
}