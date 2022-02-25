using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Mediator which wrapps handler execution into pipelines
    /// </summary>
    public class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;

        public Mediator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceProvider.GetPipeline(message.GetType());
            var context = CreateContext(message, cancellationToken);
            try
            {
                await ProcessPipeline(pipeline, context);

                var success = !context.HasError();
                return new MediatorResponse(success, context.Results, context.ErrorMessages);
            }
            catch (Exception e)
            {
                context.AddError(e.Message);
                return new MediatorResponse(false, context.Results, context.ErrorMessages);
            }
        }

        public async Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceProvider.GetPipeline(message.GetType());
            var context = CreateContext(message, cancellationToken);

            await ProcessPipeline(pipeline, context);

            if (context.HasError())
            {
                throw MediatorExecutionException.CreateForUnhandledError(context);
            }
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceProvider.GetPipeline(request.GetType());
            var context = CreateContext(request, cancellationToken);
            try
            {
                await ProcessPipeline(pipeline, context);

                var success = !context.HasError() && context.Results.Any(r => r is TResult);
                return new MediatorResponse<TResult>(success, context.Results, context.ErrorMessages);
            }
            catch (Exception e)
            {
                context.AddError(e.Message);
                return new MediatorResponse<TResult>(false, context.Results, context.ErrorMessages);
            }
        }

        public async Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            var pipeline = _serviceProvider.GetPipeline(request.GetType());
            var context = CreateContext(request, cancellationToken);
            await ProcessPipeline(pipeline, context);
            if (context.HasError())
            {
                throw MediatorExecutionException.CreateForUnhandledError(context);
            }
            if (!context.Results.Any(r => r is TResult))
            {
                throw MediatorExecutionException.CreateForMissingResult(context, typeof(TResult));
            }
            var result = context.Results
                .Where(r => r is TResult)
                .Cast<TResult>()
                .FirstOrDefault();
            if (result == null)
            {
                throw new MediatorExecutionException($"No result matching type {typeof(TResult)} was returned from pipeline", context);
            }
            return result;
        }

        private async Task ProcessPipeline(IEnumerable<IMediatorMiddleware> pipeline, MediatorContext context)
        {
            var enumerator = pipeline.GetEnumerator();
            Task next(MediatorContext ctx)
            {
                if (enumerator.MoveNext())
                {
                    return enumerator.Current.Invoke(ctx, next);
                }
                return Task.CompletedTask;
            };
            await next(context);
        }

        private MediatorContext CreateContext(IMediatorAction action, CancellationToken cancellationToken)
        {
            return new MediatorContext(action, cancellationToken);
        }
    }
}