using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Pipeline executing multiple handlers implementing TMarker type. All handlers are executed asynchronously at the same time
    /// </summary>
    public class MultiHandlerConcurrentExecutionMiddleware : ExecutionMiddleware
    {
        private readonly ServiceResolver _handlerResolver;

        public MultiHandlerConcurrentExecutionMiddleware(ServiceResolver handlerResolver)
        {
            _handlerResolver = handlerResolver;
        }

        public override bool ExecuteMultipleHandlers => true;

        protected override async Task HandleMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        {
            var handlers = _handlerResolver.GetMessageHandlers(message?.GetType());
            if (handlers.Length == 0)
            {
                throw new Exception("No handler was found for " + message?.GetType());
            }

            var tasks = handlers
                .Select(handler => ExecuteMessage(handler, message, cancellationToken))
                .ToArray();
            await Task.WhenAll(tasks);
        }
        protected override async Task HandleRequest<TRequest>(TRequest request, MediatorResponse response, CancellationToken cancellationToken)
        {
            var resultType = GenericHelpers.GetRequestResultType(request?.GetType());
            var handlers = _handlerResolver.GetRequestHandlers(request?.GetType(), resultType);
            if (handlers.Length == 0)
            {
                throw new Exception("No handler was found for " + request?.GetType());
            }

            var tasks = handlers
                .Select(async handler =>
                {
                    var resp = new MediatorResponse();
                    await ExecuteRequest(handler, request, resp, cancellationToken);
                    return resp;
                })
                .ToArray();
            var tasksResults = await Task.WhenAll(tasks);
            foreach (var taskResult in tasksResults)
            {
                if (taskResult != null)
                {
                    response.Results.AddRange(taskResult.Results);
                }
            }

        }
    }
}