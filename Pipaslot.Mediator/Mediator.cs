using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Services;

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
            static Task Seed(MediatorContext response) => Task.CompletedTask;
            var context = new MediatorContext();
            try
            {
                await pipeline
                    .Reverse()
                    .Aggregate((MiddlewareDelegate)Seed,
                        (next, middleware) => (MediatorContext res) => middleware.Invoke(message, res, next, cancellationToken))(context);

                return new MediatorResponse(context.Results, context.ErrorMessages);
            }
            catch (Exception e)
            {
                return new MediatorResponse(e.Message);
            }
        }

        public async Task<IMediatorResponse<TResponse>> Execute<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _handlerResolver.GetPipeline(request.GetType());
            static Task Seed(MediatorContext res) => Task.CompletedTask;
            var context = new MediatorContext();
            try
            {
                await pipeline
                    .Reverse()
                    .Aggregate((MiddlewareDelegate)Seed,
                        (next, middleware) => (MediatorContext res) => middleware.Invoke(request, res, next, cancellationToken))(context);

                return new MediatorResponse<TResponse>(context.Results, context.ErrorMessages);
            }
            catch (Exception e)
            {
                return new MediatorResponse<TResponse>(e.Message);
            }
        }
    }
}