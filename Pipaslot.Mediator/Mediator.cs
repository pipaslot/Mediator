using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
/// Mediator which wraps handler execution into pipelines
/// </summary>
internal class Mediator(IServiceProvider serviceProvider, MediatorContextAccessor? mediatorContextAccessor, MediatorConfigurator configurator, ILogger<Mediator> logger)
    : IMediator
{
    /// <summary>
    /// Safe-by-default message returned to <see cref="Dispatch"/>/<see cref="Execute{TResult}"/> callers for any exception
    /// that no registered <see cref="IMediatorExceptionHandler{TException}"/> translated. The original exception is only
    /// ever written to the server log (see <see cref="HandleCaughtException"/>), never to <see cref="MediatorContext.Results"/>.
    /// </summary>
    internal const string GenericErrorMessage = "An unexpected error occurred while processing the request.";

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
                throw MediatorNoHandlerFoundException.Create(message.GetType(), context);
            }

            return new MediatorResponse(context.Status == ExecutionStatus.Succeeded, context.Results);
        }
        catch (Exception e)
        {
            await HandleCaughtException(e, context).ConfigureAwait(false);
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
            throw MediatorNoHandlerFoundException.Create(message.GetType(), context);
        }

        if (context.Status != ExecutionStatus.Succeeded)
        {
            ThrowForFailedStatus(context);
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
                throw MediatorNoHandlerFoundException.Create(request.GetType(), context);
            }

            var success = context.Status == ExecutionStatus.Succeeded;
            var response = new MediatorResponse<TResult>(success, context.Results);
            if (success && !response.HasResult<TResult>())
            {
                // Route through the same catch below as every other boundary failure, instead of building the
                // response inline, so MediatorMissingResultException gets the same typed-handler/safe-by-default
                // treatment (and log) as MediatorNoHandlerFoundException a few lines above.
                throw MediatorMissingResultException.Create(typeof(TResult), context);
            }

            return response;
        }
        catch (Exception e)
        {
            await HandleCaughtException(e, context).ConfigureAwait(false);
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
            throw MediatorNoHandlerFoundException.Create(request.GetType(), context);
        }

        var success = context.Status == ExecutionStatus.Succeeded;
        var response = new MediatorResponse(success, context.Results);
        var hasResult = response.HasResult<TResult>();
        if (success && !hasResult)
        {
            throw MediatorMissingResultException.Create(typeof(TResult), context);
        }

        if (!success)
        {
            ThrowForFailedStatus(context);
        }

        var result = response.GetResult<TResult>();
        if (result is null && !hasResult)
        {
            // There was not result, neither the ActionNullResult causing the null gets accepted as expected result
            throw new MediatorExecutionException($"No result matching type {typeof(TResult)} was returned from the pipeline.", context);
        }

        return result;
    }

    /// <summary>
    /// Boundary used by DispatchUnhandled/ExecuteUnhandled once the pipeline finished with a non-succeeded status:
    /// rethrows the original exception(s) recorded via <see cref="MediatorContext.AddException"/> instead
    /// of wrapping them in the generic <see cref="MediatorUnhandledErrorException"/>, which remains the fallback for
    /// legacy middlewares that only ever set the status/message via <c>AddError</c>.
    /// </summary>
    [DoesNotReturn]
    private static void ThrowForFailedStatus(MediatorContext context)
    {
        var exceptions = context.Exceptions;
        if (exceptions.Count == 1)
        {
            ExceptionDispatchInfo.Capture(exceptions.First()).Throw();
        }

        if (exceptions.Count > 1)
        {
            throw new AggregateException(exceptions);
        }

        throw MediatorUnhandledErrorException.Create(context);
    }

    /// <summary>
    /// Boundary used by Dispatch/Execute once an exception has been caught: the single catching point for the whole
    /// library (per the entry-point contract), replacing the previous 1:1 <c>context.AddError(e.Message)</c>.
    /// A registered <see cref="IMediatorExceptionHandler{TException}"/> gets first refusal at translating the exception into
    /// a client-safe message; anything it doesn't claim falls back to the safe-by-default rules below.
    /// </summary>
    private async Task HandleCaughtException(Exception exception, MediatorContext context)
    {
        if (await TryTranslate(exception, context).ConfigureAwait(false))
        {
            return;
        }

        if (exception is MediatorUnhandledErrorException)
        {
            // The inner (nested) pipeline that threw this wrapper already propagated its own translated error
            // content into this context's Results via NotificationPropagationMiddleware before it threw - adding
            // e.Message here would duplicate that content behind a generic technical wrapper sentence .
            context.Status = ExecutionStatus.Failed;
            logger.LogWarning(exception, "Nested unhandled Mediator call failed for action '{Action}'.", context.ActionIdentifier);
            return;
        }

        // Unmapped exception, or a genuine configuration/code bug (MediatorNoHandlerFoundException, MediatorMissingResultException):
        // both stay "unexpected" by default - full detail to the log only, a generic message to the client.
        logger.LogError(exception, "Unhandled exception while processing Mediator action '{Action}'.", context.ActionIdentifier);
        context.AddError(GenericErrorMessage);
    }

    /// <summary>
    /// Resolves and invokes a typed exception handler for the caught exception's runtime type, if one is registered.
    /// Logs at Warning (with the full original exception) before applying the translation, so the log always retains
    /// the original detail even though the client only ever sees the translated message.
    /// </summary>
    private async Task<bool> TryTranslate(Exception exception, MediatorContext context)
    {
        var executor = serviceProvider.GetExceptionHandlerExecutor(configurator.ExceptionHandlerCache, exception.GetType());
        if (executor is null)
        {
            return false;
        }

        var exceptionContext = new MediatorExceptionContext(exception, context);
        var ranToCompletion = await executor.Handle(exception, serviceProvider, exceptionContext).ConfigureAwait(false);
        if (!ranToCompletion || !exceptionContext.IsHandled)
        {
            return false;
        }

        logger.LogWarning(exception, "Mediator action '{Action}' failed with an exception translated by a registered exception handler.", context.ActionIdentifier);
        context.AddError(exceptionContext.Message ?? GenericErrorMessage);
        return true;
    }

    internal List<MiddlewarePair> GetPipeline(IMediatorAction action, bool hasParentContext)
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
        var contextsCount = mediatorContextAccessor?.Push(context) ?? 1;
        context.SetDepth(contextsCount);
        var pipeline = GetPipeline(action, hasParentContext: context.IsNested);

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
        return new MediatorContext(this, mediatorContextAccessor, serviceProvider, configurator.ReflectionCache, action, cancellationToken, null, null);
    }

    internal record MiddlewarePair(IMediatorMiddleware? Instance, Type ResolvableType, object[]? Parameters);
}