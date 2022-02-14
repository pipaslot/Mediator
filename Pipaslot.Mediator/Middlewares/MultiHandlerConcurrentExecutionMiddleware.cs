using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Pipeline executing multiple handlers implementing TMarker type. All handlers are executed asynchronously at the same time
    /// </summary>
    public class MultiHandlerConcurrentExecutionMiddleware : ExecutionMiddleware
    {
        public MultiHandlerConcurrentExecutionMiddleware(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool ExecuteMultipleHandlers => true;

        protected override async Task HandleMessage(MediatorContext context)
        {
            var handlers = GetMessageHandlers(context.Action.GetType());
            if (handlers.Length == 0)
            {
                throw MediatorException.CreateForNoHandler(context.Action.GetType());
            }

            var tasks = handlers
                .Select(handler => ExecuteMessage(handler, context))
                .ToArray();
            await Task.WhenAll(tasks);
        }
        protected override async Task HandleRequest(MediatorContext context)
        {
            var handlers = GetRequestHandlers(context.Action.GetType());
            if (handlers.Length == 0)
            {
                throw MediatorException.CreateForNoHandler(context.Action.GetType());
            }

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
    }
}