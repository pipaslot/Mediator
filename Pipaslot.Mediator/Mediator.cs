using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
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
    internal class Mediator : IMediator
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MediatorContextAccessor _mediatorContextAccessor;
        private readonly MediatorConfigurator _configurator;

        public Mediator(IServiceProvider serviceProvider, MediatorContextAccessor mediatorContextAccessor, MediatorConfigurator configurator)
        {
            _serviceProvider = serviceProvider;
            _mediatorContextAccessor = mediatorContextAccessor;
            _configurator = configurator;
        }

        public async Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var pipeline = GetPipeline(message);
            var context = CreateContext(message, cancellationToken);
            try
            {
                await ProcessPipeline(pipeline, context);
                if (context.Status == ExecutionStatus.NoHandlerFound)
                {
                    throw MediatorException.CreateForNoHandler(message.GetType());
                }
                var success = !context.HasError();
                return new MediatorResponse(success, context.Results);
            }
            catch (Exception e)
            {
                context.AddError(e.Message);
                return new MediatorResponse(false, context.Results);
            }
        }

        public async Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var pipeline = GetPipeline(message);
            var context = CreateContext(message, cancellationToken);

            await ProcessPipeline(pipeline, context);
            if (context.Status == ExecutionStatus.NoHandlerFound)
            {
                throw MediatorException.CreateForNoHandler(message.GetType());
            }
            if (context.HasError())
            {
                throw MediatorExecutionException.CreateForUnhandledError(context);
            }
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var pipeline = GetPipeline(request);
            var context = CreateContext(request, cancellationToken);
            try
            {
                await ProcessPipeline(pipeline, context);
                if(!context.Results.Any(r => r is TResult))
                {
                    return new MediatorResponse<TResult>(MediatorExecutionException.CreateForMissingResult(context, typeof(TResult)).Message);
                }
                var success = !context.HasError();
                return new MediatorResponse<TResult>(success, context.Results);
            }
            catch (Exception e)
            {
                context.AddError(e.Message);
                return new MediatorResponse<TResult>(false, context.Results);
            }
        }

        public async Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var pipeline = GetPipeline(request);
            var context = CreateContext(request, cancellationToken);
            await ProcessPipeline(pipeline, context);
            if (!context.Results.Any(r => r is TResult))
            {
                throw MediatorExecutionException.CreateForMissingResult(context, typeof(TResult));
            }
            if (context.HasError())
            {
                throw MediatorExecutionException.CreateForUnhandledError(context);
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

        internal IEnumerable<IMediatorMiddleware> GetPipeline(IMediatorAction action)
        {
            var middlewareTypes = _configurator.GetMiddlewares(action, _serviceProvider);
            foreach (var middlewareType in middlewareTypes)
            {
                var middlewareInstance = (IMediatorMiddleware)_serviceProvider.GetRequiredService(middlewareType);
                yield return middlewareInstance;
                if (middlewareInstance is IExecutionMiddleware)
                {
                    yield break;
                }
            }

            yield return _serviceProvider.GetRequiredService<IExecutionMiddleware>();

        }

        private async Task ProcessPipeline(IEnumerable<IMediatorMiddleware> pipeline, MediatorContext context)
        {
            _mediatorContextAccessor.Push(context);
            try
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
            finally
            {
                _mediatorContextAccessor.Pop();
            }
        }

        private MediatorContext CreateContext(IMediatorAction action, CancellationToken cancellationToken)
        {
            return new MediatorContext(this, _mediatorContextAccessor, _serviceProvider, action, cancellationToken);
        }
    }
}