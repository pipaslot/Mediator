using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Pipeline executing one handler for request implementing TMarker type
    /// </summary>
    public class SingleHandlerExecutionMiddleware : ExecutionMiddleware
    {
        private readonly ServiceResolver _handlerResolver;

        public SingleHandlerExecutionMiddleware(ServiceResolver handlerResolver)
        {
            _handlerResolver = handlerResolver;
        }

        public override bool ExecuteMultipleHandlers => false;

        protected override async Task HandleMessage<TMessage>(TMessage message, CancellationToken cancellationToken)
        {
            var handlers = _handlerResolver.GetMessageHandlers(message?.GetType());
            if (handlers.Length > 1)
            {
                throw new Exception($"Multiple handlers were registered for the same request. Remove one from defined type: {string.Join(" OR ", handlers)}");
            }

            var handler = handlers.FirstOrDefault();
            if (handler == null)
            {
                throw new Exception("No handler was found for " + message?.GetType());
            }
            await ExecuteMessage(handler, message, cancellationToken);
        }

        protected override async Task HandleRequest<TRequest>(TRequest request, MediatorResponse response, CancellationToken cancellationToken)
        {
            var resultType = GenericHelpers.GetRequestResultType(request?.GetType());
            var handlers = _handlerResolver.GetRequestHandlers(request?.GetType(), resultType);
            if (handlers.Length > 1)
            {
                throw new Exception($"Multiple handlers were registered for the same request. Remove one from defined type: {string.Join(" OR ", handlers)}");
            }

            var handler = handlers.FirstOrDefault();
            if (handler == null)
            {
                throw new Exception("No handler was found for " + request?.GetType());
            }
            await ExecuteRequest(handler, request, response, cancellationToken);
        }
    }
}