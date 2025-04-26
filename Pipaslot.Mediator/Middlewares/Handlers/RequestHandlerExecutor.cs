using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares.Handlers;

internal class RequestHandlerExecutor<TRequest, TResult> : HandlerExecutor
    where TRequest : IMediatorAction<TResult>
{
    public override Task Execute(MediatorContext context)
    {
        var handlers = context.Services.GetServices<IMediatorHandler<TRequest, TResult>>().ToArray();
        if (handlers.Any())
        {
            if (handlers.Length == 1)
            {
                // Faster execution when only single handler is available
                return ExecuteRequest(handlers.First(), context);
            }

            var actionType = context.Action.GetType();
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

    private async Task ExecuteRequestInSequence(MediatorContext context, IMediatorHandler<TRequest, TResult>[] handlers)
    {
        var sortedHandlers = Sort(handlers);
        foreach (var handler in sortedHandlers)
        {
            await ExecuteRequest(handler, context).ConfigureAwait(false);
        }
    }

    private async Task ExecuteRequestConcurrent(MediatorContext context, IMediatorHandler<TRequest, TResult>[] handlers)
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
            context.Append(taskResult);
        }
    }

    /// <summary>
    /// Execute handler
    /// </summary>
    private async Task ExecuteRequest(IMediatorHandler<TRequest, TResult> handler, MediatorContext context)
    {
        try
        {
            var result = await handler.Handle((TRequest)context.Action, context.CancellationToken).ConfigureAwait(false);
            if (result is not null)
            {
                context.AddResult(result);
            }
            else
            {
                context.AddResult(new NullActionResult());
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
}