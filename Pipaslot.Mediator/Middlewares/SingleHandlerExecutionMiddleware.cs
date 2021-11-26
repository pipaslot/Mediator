using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Pipeline executing one handler for request implementing TMarker type
    /// </summary>
    public class SingleHandlerExecutionMiddleware : ExecutionMiddleware
    {
        private readonly IServiceProvider _serviceProvider;

        public SingleHandlerExecutionMiddleware(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override bool ExecuteMultipleHandlers => false;

        protected override async Task HandleMessage<TMessage>(TMessage message, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = _serviceProvider.GetMessageHandlers(message?.GetType());
            if (handlers.Length > 1)
            {
                throw new Exception($"Multiple handlers were registered for the same request. Remove one from defined type: {string.Join(" OR ", handlers)}");
            }

            var handler = handlers.FirstOrDefault();
            if (handler == null)
            {
                throw new Exception("No handler was found for " + message?.GetType());
            }
            await ExecuteMessage(handler, message, context, cancellationToken);
        }

        protected override async Task HandleRequest<TRequest>(TRequest request, MediatorContext context, CancellationToken cancellationToken)
        {
            var resultType = RequestGenericHelpers.GetRequestResultType(request?.GetType());
            var handlers = _serviceProvider.GetRequestHandlers(request?.GetType(), resultType);
            if (handlers.Length > 1)
            {
                throw new Exception($"Multiple handlers were registered for the same request. Remove one from defined type: {string.Join(" OR ", handlers)}");
            }

            var handler = handlers.FirstOrDefault();
            if (handler == null)
            {
                throw new Exception("No handler was found for " + request?.GetType());
            }
            await ExecuteRequest(handler, request, context, cancellationToken);
        }
    }
}