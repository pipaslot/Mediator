using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Pipeline executing multiple handlers implementing TMarker type. Handlers are executed in row, once previous execution finished.
    /// For order specification see <see cref="ISequenceHandler"/>
    /// </summary>
    public class MultiHandlerSequenceExecutionMiddleware : ExecutionMiddleware
    {
        private readonly ServiceResolver _handlerResolver;

        public MultiHandlerSequenceExecutionMiddleware(ServiceResolver handlerResolver)
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
            var sortedHandlers = Sort(handlers);
            foreach (var handler in sortedHandlers)
            {
                await ExecuteMessage(handler, message, cancellationToken);
            }
        }
        protected override async Task HandleRequest<TRequest>(TRequest request, MediatorResponse response, CancellationToken cancellationToken)
        {
            var resultType = GenericHelpers.GetRequestResultType(request?.GetType());
            var handlers = _handlerResolver.GetRequestHandlers(request?.GetType(), resultType);
            if (handlers.Length == 0)
            {
                throw new Exception("No handler was found for " + request?.GetType());
            }
            var sortedHandlers = Sort(handlers);
            foreach (var handler in sortedHandlers)
            {
                await ExecuteRequest(handler, request, response, cancellationToken);
            }
        }

        private object[] Sort(object[] handlers)
        {
            return handlers
                .Select(h => new
                {
                    Handler = h,
                    Order = (h is ISequenceHandler s) ? s.Order : int.MaxValue
                })
                .OrderBy(i=>i.Order)
                .Select(i=>i.Handler)
                .ToArray();
        }
    }
}