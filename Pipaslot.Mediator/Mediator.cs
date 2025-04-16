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

    internal List<MiddlewarePair> GetPipeline(IMediatorAction action, MediatorContext context, bool hasParentContext)
    {
        var res = new List<MiddlewarePair>(5);
        if (hasParentContext)
        {
            // As performance optimization, we apply the propagation middleware only if there is any parent for the propagation
            res.Add(new MiddlewarePair(NotificationPropagationMiddleware.Instance, typeof(NotificationPropagationMiddleware), null));
        }

        configurator.CollectMiddlewares(action, serviceProvider, res);

        res.Add(new MiddlewarePair(null, typeof(IExecutionMiddleware), null));
        return res;
    }

    private Task ProcessPipeline(IMediatorAction action, MediatorContext context)
    {
        var contextsCount = mediatorContextAccessor.Push(context); // Processing time: 80ns, Allocation: 448B
        var pipeline = GetPipeline(action, context, hasParentContext: contextsCount > 1);

        var index = -1;
        return Next(context);
        Task Next(MediatorContext ctx)
        {
            index++;
            if (index >= pipeline.Count)
            {
                return Task.CompletedTask;
            }
        
            var current = pipeline[index];
        
            if (current.Parameters is not null)
            {
                ctx.Features.Set(new MiddlewareParametersFeature(current.Parameters));
            }
            else if (ctx.FeaturesAreInitialized)// Avoid feature collection initialization as the MiddlewareParametersFeature is provided as default parameter always available during reading
            {
                // Reset parameters as we are executing different middleware
                ctx.Features.Set(MiddlewareParametersFeature.Default);
            }

            var instance = current.Instance ?? (IMediatorMiddleware)serviceProvider.GetRequiredService(current.ResolvableType);
            return instance.Invoke(ctx, Next);
        }
    }

    private MediatorContext CreateContext(IMediatorAction action, CancellationToken cancellationToken)
    {
        return new MediatorContext(this, mediatorContextAccessor, serviceProvider, action, cancellationToken, null, null);
    }

    internal readonly record  struct MiddlewarePair(IMediatorMiddleware? Instance, Type ResolvableType, object[]? Parameters);
}