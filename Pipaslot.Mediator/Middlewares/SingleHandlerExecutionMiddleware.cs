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

        protected override async Task HandleMessage<TMessage>(TMessage message, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetMessageHandlers(message?.GetType());
            if (handlers.Length > 1)
            {
                throw MediatorException.CreateForDuplicateHandlers(handlers);
            }

            var handler = handlers.FirstOrDefault();
            if (handler == null)
            {
                throw MediatorException.CreateForNoHandler(message?.GetType());
            }
            await ExecuteMessage(handler, message, context, cancellationToken);
        }

        protected override async Task HandleRequest<TRequest>(TRequest request, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetRequestHandlers(request?.GetType());
            if (handlers.Length > 1)
            {
                throw MediatorException.CreateForDuplicateHandlers(handlers);
            }

            var handler = handlers.FirstOrDefault();
            if (handler == null)
            {
                throw MediatorException.CreateForNoHandler(request?.GetType());
            }
            await ExecuteRequest(handler, request, context, cancellationToken);
        }
    }
}