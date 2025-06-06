﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares.Handlers;

internal class RequestHandlerExecutor<TRequest, TResult> : HandlerExecutor
    where TRequest : IMediatorAction<TResult>
{
    private IMediatorHandler<TRequest, TResult>[]? _handlers;
    public override Task Execute(MediatorContext context)
    {
        var handlers = ResolveHandlers(context.Services);
        if (handlers.Any())
        {
            if (handlers.Length == 1)
            {
                // Faster execution when only single handler is available
                return ExecuteRequest(handlers[0], context);
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
    
    private IMediatorHandler<TRequest, TResult>[] ResolveHandlers(IServiceProvider services)
    {
        _handlers ??= services.GetServices<IMediatorHandler<TRequest, TResult>>().ToArray();
        return _handlers;
    }
    
    internal override object[] GetHandlers(IServiceProvider services)
    {
        return ResolveHandlers(services);
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
        catch (Exception)
        {
            context.Status = ExecutionStatus.Failed;
            throw;
        }
    }
}