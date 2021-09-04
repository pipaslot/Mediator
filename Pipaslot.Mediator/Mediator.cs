using System;
using System.Collections.Generic;
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
        private readonly ServiceResolver _serviceResolver;

        public Mediator(ServiceResolver handlerResolver)
        {
            _serviceResolver = handlerResolver;
        }

        public async Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceResolver.GetPipeline(message.GetType());
            var context = CreateContext();
            try
            {
                await ProcessPipeline(pipeline, message, context, cancellationToken);

                var success = context.ErrorMessages.Count == 0;
                return new MediatorResponse(success, context.Results, context.ErrorMessagesDistincted);
            }
            catch (Exception e)
            {
                context.ErrorMessages.Add(e.Message);
                return new MediatorResponse(false, context.Results, context.ErrorMessagesDistincted);
            }
        }

        public async Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceResolver.GetPipeline(message.GetType());
            var context = CreateContext();
            await ProcessPipeline(pipeline, message, context, cancellationToken);

            if (context.ErrorMessages.Any())
            {
                throw new MediatorExecutionException(context.ErrorMessagesDistincted);
            }
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceResolver.GetPipeline(request.GetType());
            var context = CreateContext();
            try
            {
                await ProcessPipeline(pipeline, request, context, cancellationToken);

                var success = context.ErrorMessages.Count == 0 && context.Results.Any(r => r is TResult);
                return new MediatorResponse<TResult>(success, context.Results, context.ErrorMessagesDistincted);
            }
            catch (Exception e)
            {
                context.ErrorMessages.Add(e.Message);
                return new MediatorResponse<TResult>(false, context.Results, context.ErrorMessagesDistincted);
            }
        }

        public async Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceResolver.GetPipeline(request.GetType());
            var context = CreateContext();
            await ProcessPipeline(pipeline, request, context, cancellationToken);

            var success = context.ErrorMessages.Count == 0 && context.Results.Any(r => r is TResult);
            if (context.ErrorMessages.Any())
            {
                throw new MediatorExecutionException(context.ErrorMessagesDistincted);
            }
            var result = context.Results
                .Where(r => r is TResult)
                .Cast<TResult>()
                .FirstOrDefault();
            return result;
        }

        private async Task ProcessPipeline<TAction>(IEnumerable<IMediatorMiddleware> pipeline, TAction request, MediatorContext context, CancellationToken cancellationToken)
        {
            static Task Seed(MediatorContext res) => Task.CompletedTask;

            await pipeline
                .Reverse()
                .Aggregate((MiddlewareDelegate)Seed,
                    (next, middleware) => (res) => middleware.Invoke(request, res, next, cancellationToken))(context);
        }

        private MediatorContext CreateContext()
        {
            return new MediatorContext(this);
        }
    }
}