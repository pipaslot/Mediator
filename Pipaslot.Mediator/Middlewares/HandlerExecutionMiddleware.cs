using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Services;
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
                if (e.InnerException != null)
                {
                    // Unwrap exception
                    context.AddError(e.InnerException.Message);
                    throw e.InnerException;
                }

                throw;
            }
            catch (Exception e)
            {
                context.AddError(e.Message);
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
                if (e.InnerException != null)
                {
                    context.AddError(e.InnerException.Message);
                    // Unwrap exception
                    throw e.InnerException;
                }

                throw;
            }
            catch (Exception e)
            {
                context.AddError(e.Message);
                throw;
            }
        }

        private async Task HandleMessage(MediatorContext context)
        {
            var actionType = context.Action.GetType();
            var handlers = _serviceProvider.GetMessageHandlers(actionType);
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

        private async Task HandleRequest(MediatorContext context)
        {
            var actionType = context.Action.GetType();
            var resultType = RequestGenericHelpers.GetRequestResultType(actionType);
            var handlers = _serviceProvider.GetRequestHandlers(actionType, resultType);
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

        internal static bool ValidateHandlers(object[] handlers, Type actionType)
        {
            if (!handlers.Any())
            {
                throw MediatorException.CreateForNoHandler(actionType);
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