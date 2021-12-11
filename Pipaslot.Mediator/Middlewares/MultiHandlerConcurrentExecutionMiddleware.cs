using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;
using System.Threading;
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

        protected override async Task HandleMessage<TMessage>(TMessage message, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetMessageHandlers(message?.GetType());
            if (handlers.Length == 0)
            {
                throw MediatorException.CreateForNoHandler(message?.GetType());
            }

            var tasks = handlers
                .Select(handler => ExecuteMessage(handler, message, context, cancellationToken))
                .ToArray();
            await Task.WhenAll(tasks);
        }
        protected override async Task HandleRequest<TRequest>(TRequest request, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetRequestHandlers(request?.GetType());
            if (handlers.Length == 0)
            {
                throw MediatorException.CreateForNoHandler(request?.GetType());
            }

            var tasks = handlers
                .Select(async handler =>
                {
                    var resp = context.CopyEmpty();
                    await ExecuteRequest(handler, request, resp, cancellationToken);
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