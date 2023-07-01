﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
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

        public async Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default, IFeatureCollection? defaultFeatures = null)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var pipeline = GetPipeline(message);
            var context = CreateContext(message, cancellationToken, defaultFeatures);
            try
            {
                await ProcessPipeline(pipeline, context).ConfigureAwait(false);
                if (context.Status == ExecutionStatus.NoHandlerFound)
                {
                    throw MediatorExecutionException.CreateForNoHandler(message.GetType(), context);
                }
                return new MediatorResponse(context.Status == ExecutionStatus.Succeeded, context.Results);
            }
            catch (Exception e)
            {
                context.AddError(e.Message);
                return new MediatorResponse(false, context.Results);
            }
        }

        public async Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default, IFeatureCollection? defaultFeatures = null)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            var pipeline = GetPipeline(message);
            var context = CreateContext(message, cancellationToken, defaultFeatures);

            await ProcessPipeline(pipeline, context).ConfigureAwait(false);
            if (context.Status == ExecutionStatus.NoHandlerFound)
            {
                throw MediatorExecutionException.CreateForNoHandler(message.GetType(), context);
            }
            if (context.Status != ExecutionStatus.Succeeded)
            {
                throw MediatorExecutionException.CreateForUnhandledError(context);
            }
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default, IFeatureCollection? defaultFeatures = null)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var pipeline = GetPipeline(request);
            var context = CreateContext(request, cancellationToken, defaultFeatures);
            try
            {
                await ProcessPipeline(pipeline, context).ConfigureAwait(false);
                //If somebody wants to provide result event if there is no handler, then they should change the Context.Status or the HandlerExecutionMiddleware shouldnt be executed
                if (context.Status == ExecutionStatus.NoHandlerFound)
                {
                    throw MediatorExecutionException.CreateForNoHandler(request.GetType(), context);
                }
                var success = context.Status == ExecutionStatus.Succeeded;
                if (success && !context.Results.Any(r => r is TResult))
                {
                    return new MediatorResponse<TResult>(MediatorExecutionException.CreateForMissingResult(context, typeof(TResult)).Message);
                }
                return new MediatorResponse<TResult>(success, context.Results);
            }
            catch (Exception e)
            {
                context.AddError(e.Message);
                return new MediatorResponse<TResult>(false, context.Results);
            }
        }

        public async Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default, IFeatureCollection? defaultFeatures = null)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var pipeline = GetPipeline(request);
            var context = CreateContext(request, cancellationToken, defaultFeatures);
            await ProcessPipeline(pipeline, context).ConfigureAwait(false);
            //If somebody wants to provide result event if there is no handler, then they should change the Context.Status or the HandlerExecutionMiddleware shouldnt be executed
            if (context.Status == ExecutionStatus.NoHandlerFound)
            {
                throw MediatorExecutionException.CreateForNoHandler(request.GetType(), context);
            }
            var success = context.Status == ExecutionStatus.Succeeded;
            if (success && !context.Results.Any(r => r is TResult))
            {
                throw MediatorExecutionException.CreateForMissingResult(context, typeof(TResult));
            }
            if (!success)
            {
                throw MediatorExecutionException.CreateForUnhandledError(context);
            }
            var result = context.Results
                .Where(r => r is TResult)
                .Cast<TResult>()
                .FirstOrDefault()
                ?? throw new MediatorExecutionException($"No result matching type {typeof(TResult)} was returned from the pipeline.", context);
            return result;
        }

        internal IEnumerable<(IMediatorMiddleware Instance, object[]? Parameters)> GetPipeline(IMediatorAction action)
        {
            var middlewareDefinitions = _configurator.GetMiddlewares(action, _serviceProvider);
            foreach (var middlewareDefinition in middlewareDefinitions)
            {
                var middlewareInstance = (IMediatorMiddleware)_serviceProvider.GetRequiredService(middlewareDefinition.Type);
                yield return (middlewareInstance, middlewareDefinition.Parameters);
                if (middlewareInstance is IExecutionMiddleware)
                {
                    yield break;
                }
            }

            yield return (_serviceProvider.GetRequiredService<IExecutionMiddleware>(), null);

        }

        private async Task ProcessPipeline(IEnumerable<(IMediatorMiddleware Instance, object[]? Parameters)> pipeline, MediatorContext context)
        {
            _mediatorContextAccessor.Push(context);
            try
            {
                var enumerator = pipeline.GetEnumerator();
                Task next(MediatorContext ctx)
                {
                    if (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        if(current.Parameters == null)
                        {
                            var feature = ctx.Features.Get<MiddlewareParametersFeature>();
                            // Prevent increasing revision number when not necessary
                            if(feature != MiddlewareParametersFeature.Default)
                            {
                                ctx.Features.Set(MiddlewareParametersFeature.Default);
                            }
                        }
                        else
                        {
                            var feature = new MiddlewareParametersFeature(current.Parameters);
                            ctx.Features.Set(feature);
                        }
                        return current.Instance.Invoke(ctx, next);
                    }
                    return Task.CompletedTask;
                };
                await next(context).ConfigureAwait(false);
            }
            finally
            {
                _mediatorContextAccessor.Pop();
            }
        }

        private MediatorContext CreateContext(IMediatorAction action, CancellationToken cancellationToken, IFeatureCollection? defaultFeatures)
        {
            return new MediatorContext(this, _mediatorContextAccessor, _serviceProvider, action, cancellationToken, null, defaultFeatures);
        }
    }
}