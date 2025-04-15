using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
/// Mediator which wraps handler execution into pipelines
/// </summary>
internal class Mediator(IServiceProvider serviceProvider, MediatorContextAccessor mediatorContextAccessor, MediatorConfigurator configurator)
    : IMediator
{
    public async Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var context = CreateContext(message, cancellationToken);
        try
        {
            await ProcessPipeline(message, context).ConfigureAwait(false);
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

    public async Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
    {
        if (message is null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var context = CreateContext(message, cancellationToken);

        await ProcessPipeline(message, context).ConfigureAwait(false);
        if (context.Status == ExecutionStatus.NoHandlerFound)
        {
            throw MediatorExecutionException.CreateForNoHandler(message.GetType(), context);
        }

        if (context.Status != ExecutionStatus.Succeeded)
        {
            throw MediatorExecutionException.CreateForUnhandledError(context);
        }
    }

    public async Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var context = CreateContext(request, cancellationToken);
        try
        {
            await ProcessPipeline(request, context).ConfigureAwait(false);
            //If somebody wants to provide result event if there is no handler, then they should change the Context.Status or the HandlerExecutionMiddleware shouldnt be executed
            if (context.Status == ExecutionStatus.NoHandlerFound)
            {
                throw MediatorExecutionException.CreateForNoHandler(request.GetType(), context);
            }

            var success = context.Status == ExecutionStatus.Succeeded;
            var response = new MediatorResponse<TResult>(success, context.Results);
            if (success && !response.HasResult<TResult>())
            {
                return new MediatorResponse<TResult>(MediatorExecutionException.CreateForMissingResult(context, typeof(TResult)).Message);
            }

            return response;
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

        var context = CreateContext(request, cancellationToken);
        await ProcessPipeline(request, context).ConfigureAwait(false);
        //If somebody wants to provide result event if there is no handler, then they should change the Context.Status or the HandlerExecutionMiddleware shouldnt be executed
        if (context.Status == ExecutionStatus.NoHandlerFound)
        {
            throw MediatorExecutionException.CreateForNoHandler(request.GetType(), context);
        }

        var success = context.Status == ExecutionStatus.Succeeded;
        var response = new MediatorResponse(success, context.Results);
        var hasResult = response.HasResult<TResult>();
        if (success && !hasResult)
        {
            throw MediatorExecutionException.CreateForMissingResult(context, typeof(TResult));
        }

        if (!success)
        {
            throw MediatorExecutionException.CreateForUnhandledError(context);
        }

        var result = response.GetResult<TResult>();
        if (result is null && !hasResult)
        {
            // There was not result, neither the ActionNullResult causing the null gets accepted as expected result
            throw new MediatorExecutionException($"No result matching type {typeof(TResult)} was returned from the pipeline.", context);
        }

        return result;
    }

    internal IEnumerable<MiddlewarePair> GetPipeline(IMediatorAction action, MediatorContext context)
    {
        if (context.ParentContexts.Length > 0)
        {
            // As performance optimization, we apply the propagation middleware only if there is any parent for the propagation
            yield return new MiddlewarePair(NotificationPropagationMiddleware.Instance, null);
        }

        var middlewareDefinitions = configurator.GetMiddlewares(action, serviceProvider);
        foreach (var middlewareDefinition in middlewareDefinitions)
        {
            var middlewareInstance = (IMediatorMiddleware)serviceProvider.GetRequiredService(middlewareDefinition.Type);
            yield return new MiddlewarePair(middlewareInstance, middlewareDefinition.Parameters);
            if (middlewareInstance is IExecutionMiddleware)
            {
                yield break;
            }
        }

        yield return new MiddlewarePair(serviceProvider.GetRequiredService<IExecutionMiddleware>(), null);
    }

    private async Task ProcessPipeline(IMediatorAction action, MediatorContext context)
    {
        var pipeline = GetPipeline(action, context);
        mediatorContextAccessor.Push(context);
        using var enumerator = pipeline.GetEnumerator();
        await Next(context).ConfigureAwait(false);
        return;
        Task Next(MediatorContext ctx)
        {
            if (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                if (current.Parameters == null)
                {
                    var feature = ctx.Features.Get<MiddlewareParametersFeature>();
                    // Prevent increasing revision number when not necessary
                    if (feature != MiddlewareParametersFeature.Default)
                    {
                        ctx.Features.Set(MiddlewareParametersFeature.Default);
                    }
                }
                else
                {
                    var feature = new MiddlewareParametersFeature(current.Parameters);
                    ctx.Features.Set(feature);
                }

                return current.Instance.Invoke(ctx, Next);
            }

            return Task.CompletedTask;
        }

    }

    private MediatorContext CreateContext(IMediatorAction action, CancellationToken cancellationToken)
    {
        return new MediatorContext(this, mediatorContextAccessor, serviceProvider, action, cancellationToken, null, null);
    }

    internal readonly record  struct MiddlewarePair(IMediatorMiddleware Instance, object[]? Parameters);
}