using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Execute handlers. No more middlewares will be executed.
/// </summary>
public class HandlerExecutionMiddleware : IExecutionMiddleware
{
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        return context.HasActionReturnValue
            ? HandleRequest(context)
            : HandleMessage(context);
    }

    #region Messages

    private Task HandleMessage(MediatorContext context)
    {
        var actionType = context.Action.GetType();
        var handlers = context.GetHandlers();
        if (handlers.Length > 0)
        {
            if (handlers.Length == 1)
            {
                // Faster execution when only single handler is available
                return ExecuteMessage(handlers.First(), context);
            }

            var runConcurrent = ValidateHandlers(handlers, actionType);
            if (runConcurrent)
            {
                var tasks = handlers
                    .Select(handler => ExecuteMessage(handler, context))
                    .ToArray();
                return Task.WhenAll(tasks);
            }

            return ExecutedMessagesInSequence(context, handlers);
        }

        context.Status = ExecutionStatus.NoHandlerFound;
        return Task.CompletedTask;
    }

    private async Task ExecutedMessagesInSequence(MediatorContext context, object[] handlers)
    {
        var sortedHandlers = Sort(handlers);
        foreach (var handler in sortedHandlers)
        {
            await ExecuteMessage(handler, context).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Execute handler
    /// </summary>
    protected async Task ExecuteMessage(object handler, MediatorContext context)
    {
        var method = handler.GetType().GetMethod(nameof(IMediatorHandler<IMediatorAction>.Handle));
        try
        {
            var task = (Task?)method!.Invoke(handler, [context.Action, context.CancellationToken])!;
            if (task != null)
            {
                await task.ConfigureAwait(false);
            }
        }
        catch (TargetInvocationException e)
        {
            context.Status = ExecutionStatus.Failed;
            if (e.InnerException != null)
            {
                // Unwrap exception
                throw e.InnerException;
            }

            throw;
        }
        catch (Exception)
        {
            context.Status = ExecutionStatus.Failed;
            throw;
        }
    }

    #endregion

    #region Requests

    private Task HandleRequest(MediatorContext context)
    {
        var actionType = context.Action.GetType();
        var handlers = context.GetHandlers();
        if (handlers.Any())
        {
            if (handlers.Length == 1)
            {
                // Faster execution when only single handler is available
                return ExecuteRequest(handlers.First(), context);
            }

            var runConcurrent = ValidateHandlers(handlers, actionType);
            if (runConcurrent)
            {
                return ExecuteRequestConcurrent(context, handlers);
            }

            return ExecuteRequestInSequence(context, handlers);
        }

        context.Status = ExecutionStatus.NoHandlerFound;
        return Task.CompletedTask;
    }

    private async Task ExecuteRequestInSequence(MediatorContext context, object[] handlers)
    {
        var sortedHandlers = Sort(handlers);
        foreach (var handler in sortedHandlers)
        {
            await ExecuteRequest(handler, context).ConfigureAwait(false);
        }
    }

    private async Task ExecuteRequestConcurrent(MediatorContext context, object[] handlers)
    {
        var tasks = handlers
            .Select(async handler =>
            {
                var resp = context.CopyEmpty();
                await ExecuteRequest(handler, resp).ConfigureAwait(false);
                return resp;
            })
            .ToArray();
        var tasksResults = await Task.WhenAll(tasks).ConfigureAwait(false);
        foreach (var taskResult in tasksResults)
        {
            if (taskResult is not null)
            {
                context.Append(taskResult);
            }
        }
    }

    /// <summary>
    /// Execute handler
    /// </summary>
    protected async Task ExecuteRequest(object handler, MediatorContext context)
    {
        var method = handler.GetType().GetMethod(nameof(IMediatorHandler<IMediatorAction<object>, object>.Handle));
        try
        {
            var task = (Task?)method!.Invoke(handler, [context.Action, context.CancellationToken])!;
            if (task != null)
            {
                await task.ConfigureAwait(false);

                var resultProperty = task.GetType().GetProperty("Result");
                var result = resultProperty?.GetValue(task);
                context.AddResult(result ?? new NullActionResult());
            }
        }
        catch (TargetInvocationException e)
        {
            context.Status = ExecutionStatus.Failed;
            if (e.InnerException != null)
            {
                // Unwrap exception
                throw e.InnerException;
            }

            throw;
        }
        catch (Exception)
        {
            context.Status = ExecutionStatus.Failed;
            throw;
        }
    }

    #endregion

    #region Common

    internal static bool ValidateHandlers(object[] handlers, Type actionType)
    {
        if (!handlers.Any())
        {
            return false;
        }

        var anyIsSequence = false;
        var anyIsConcurrent = false;
        var anyIsSingle = false;
        foreach (var handler in handlers)
        {
            var isSequence = handler is ISequenceHandler;
            var isConcurrent = handler is IConcurrentHandler;
            var isSingle = !isSequence && !isConcurrent;
            anyIsSequence = anyIsSequence || isSequence;
            anyIsConcurrent = anyIsConcurrent || isConcurrent;
            anyIsSingle = anyIsSingle || isSingle;
        }

        if ((anyIsConcurrent && anyIsSequence)
            || (anyIsConcurrent && anyIsSingle)
            || (anyIsSequence && anyIsSingle))
        {
            throw MediatorException.CreateForCanNotCombineHandlers(actionType, handlers);
        }

        if (anyIsSingle && handlers.Length > 1)
        {
            throw MediatorException.CreateForDuplicateHandlers(actionType, handlers);
        }

        return anyIsConcurrent;
    }

    private static object[] Sort(object[] handlers)
    {
        return handlers
            .Select(h => new { Handler = h, Order = h is ISequenceHandler s ? s.Order : int.MaxValue / 2 })
            .OrderBy(i => i.Order)
            .Select(i => i.Handler)
            .ToArray();
    }

    #endregion
}