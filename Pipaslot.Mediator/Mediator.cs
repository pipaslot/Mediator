using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
                        (next, middleware) => (res) => middleware.Invoke(message, res, next, cancellationToken))(context);

                var success = context.ErrorMessages.Count() == 0;
                return new MediatorResponse(success, context.Results, context.ErrorMessages);
            }
            catch (Exception e)
            {
                return new MediatorResponse(e.Message);
            }
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _handlerResolver.GetPipeline(request.GetType());
            static Task Seed(MediatorContext res) => Task.CompletedTask;
            var context = new MediatorContext();
            try
            {
                await pipeline
                    .Reverse()
                    .Aggregate((MiddlewareDelegate)Seed,
                        (next, middleware) => (res) => middleware.Invoke(request, res, next, cancellationToken))(context);

                var success = context.ErrorMessages.Count() == 0 && context.Results.Any(r => r is TResult);
                return new MediatorResponse<TResult>(success, context.Results, context.ErrorMessages);
            }
            catch (Exception e)
            {
                return new MediatorResponse<TResult>(e.Message);
            }
        }
    }
}