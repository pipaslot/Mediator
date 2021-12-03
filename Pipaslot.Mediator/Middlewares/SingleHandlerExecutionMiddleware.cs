using Pipaslot.Mediator.Abstractions;
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
        public SingleHandlerExecutionMiddleware(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool ExecuteMultipleHandlers => false;
        public override ActionToHandlerBindingType BindingType => ActionToHandlerBindingType.Class;

        protected override async Task HandleMessage<TMessage>(TMessage message, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetMessageHandlers(message?.GetType());
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
            var handlers = GetRequestHandlers(request?.GetType());
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