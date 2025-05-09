using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares.Handlers;

internal class MessageHandlerExecutor<TMessage> : HandlerExecutor
where TMessage : IMediatorAction
{
    private IMediatorHandler<TMessage>[]? _handlers;
    public override Task Execute(MediatorContext context)
    {
        var handlers = ResolveHandlers(context.Services);
        if (handlers.Length > 0)
        {
            if (handlers.Length == 1)
            {
                // Faster execution when only single handler is available
                return ExecuteMessage(handlers[0], context);
            }

            var actionType = context.Action.GetType();
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

    private IMediatorHandler<TMessage>[] ResolveHandlers(IServiceProvider services)
    {
        _handlers ??= services.GetServices<IMediatorHandler<TMessage>>().ToArray();
        return _handlers;
    }
    internal override object[] GetHandlers(IServiceProvider services)
    {
        return ResolveHandlers(services);
    }

    private async Task ExecutedMessagesInSequence(MediatorContext context, IMediatorHandler<TMessage>[] handlers)
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
    protected Task ExecuteMessage(IMediatorHandler<TMessage> handler, MediatorContext context)
    {
        try
        {
            return handler.Handle((TMessage)context.Action, context.CancellationToken);
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