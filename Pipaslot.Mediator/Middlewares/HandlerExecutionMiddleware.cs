using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Notifications;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Pipeline executing one handler for request implementing TMarker type
    /// </summary>
    public class HandlerExecutionMiddleware : IExecutionMiddleware
    {
        private readonly IServiceProvider _serviceProvider;

        public HandlerExecutionMiddleware(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            if (context.HasActionReturnValue)
            {
                await HandleRequest(context);
            }
            else
            {
                await HandleMessage(context);
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
                var task = (Task?)method!.Invoke(handler, new object[] { context.Action, context.CancellationToken })!;
                if (task != null)
                {
                    await task;
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

        /// <summary>
        /// Execute handler
        /// </summary>
        protected async Task ExecuteRequest(object handler, MediatorContext context)
        {
            var method = handler.GetType().GetMethod(nameof(IMediatorHandler<IMediatorAction<object>, object>.Handle));
            try
            {
                var task = (Task?)method!.Invoke(handler, new object[] { context.Action, context.CancellationToken })!;
                if (task != null)
                {
                    await task.ConfigureAwait(false);

                    var resultProperty = task.GetType().GetProperty("Result");
                    var result = resultProperty?.GetValue(task);
                    if (result != null)
                    {
                        context.AddResult(result);
                    }
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

        private async Task HandleMessage(MediatorContext context)
        {
            var actionType = context.Action.GetType();
            var handlers = context.GetHandlers();
            if (handlers.Any())
            {
                var runConcurrent = ValidateHandlers(handlers, actionType);
                if (runConcurrent)
                {
                    var tasks = handlers
                    .Select(handler => ExecuteMessage(handler, context))
                    .ToArray();
                    await Task.WhenAll(tasks);
                }
                else
                {
                    var sortedHandlers = Sort(handlers);
                    foreach (var handler in sortedHandlers)
                    {
                        await ExecuteMessage(handler, context);
                    }
                }
            }
            else
            {
                context.Status = ExecutionStatus.NoHandlerFound;
            }
        }

        private async Task HandleRequest(MediatorContext context)
        {
            var actionType = context.Action.GetType();
            var handlers = context.GetHandlers(); 
            if (handlers.Any())
            {
                var runConcurrent = ValidateHandlers(handlers, actionType);
                if (runConcurrent)
                {
                    var tasks = handlers
                    .Select(async handler =>
                    {
                        var resp = context.CopyEmpty();
                        await ExecuteRequest(handler, resp);
                        return resp;
                    })
                    .ToArray();
                    var tasksResults = await Task.WhenAll(tasks);
                    foreach (var taskResult in tasksResults)
                    {
                        if (taskResult != null)
                        {
                            context.Append(taskResult);
                        }
                    }
                }
                else
                {
                    var sortedHandlers = Sort(handlers);
                    foreach (var handler in sortedHandlers)
                    {
                        await ExecuteRequest(handler, context);
                    }
                }
            }
            else
            {
                context.Status = ExecutionStatus.NoHandlerFound;
            }
        }

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

        private object[] Sort(object[] handlers)
        {
            return handlers
                .Select(h => new
                {
                    Handler = h,
                    Order = (h is ISequenceHandler s) ? s.Order : int.MaxValue / 2
                })
                .OrderBy(i => i.Order)
                .Select(i => i.Handler)
                .ToArray();
        }
    }
}