using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Pipeline executing one handler for request implementing TMarker type
    /// </summary>
    public class HandlerExecutionMiddleware : ExecutionMiddleware
    {
        public HandlerExecutionMiddleware(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
        [Obsolete]
        public override bool ExecuteMultipleHandlers => true;

        protected override async Task HandleMessage(MediatorContext context)
        {
            var actionType = context.Action.GetType();
            var handlers = GetMessageHandlers(actionType);
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

        protected override async Task HandleRequest(MediatorContext context)
        {
            var actionType = context.Action.GetType();
            var handlers = GetRequestHandlers(actionType);
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
                throw MediatorException.CreateForCanNotCombineHandlers(handlers);
            }
            if (anyIsSingle && handlers.Length > 1)
            {
                throw MediatorException.CreateForDuplicateHandlers(handlers);
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