using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Pipeline executing multiple handlers implementing TMarker type. Handlers are executed in row, once previous execution finished.
    /// For order specification <see cref="ISequenceHandler"/>
    /// </summary>
    public class MultiHandlerSequenceExecutionMiddleware : ExecutionMiddleware
    {
        public MultiHandlerSequenceExecutionMiddleware(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        public override bool ExecuteMultipleHandlers => true;
        public override ActionToHandlerBindingType BindingType => ActionToHandlerBindingType.Class;

        protected override async Task HandleMessage<TMessage>(TMessage message, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetMessageHandlers(message?.GetType());
            if (handlers.Length == 0)
            {
                throw new Exception("No handler was found for " + message?.GetType());
            }
            var sortedHandlers = Sort(handlers);
            foreach (var handler in sortedHandlers)
            {
                await ExecuteMessage(handler, message, context, cancellationToken);
            }
        }
        protected override async Task HandleRequest<TRequest>(TRequest request, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetRequestHandlers(request?.GetType());
            if (handlers.Length == 0)
            {
                throw new Exception("No handler was found for " + request?.GetType());
            }
            var sortedHandlers = Sort(handlers);
            foreach (var handler in sortedHandlers)
            {
                await ExecuteRequest(handler, request, context, cancellationToken);
            }
        }

        private object[] Sort(object[] handlers)
        {
            return handlers
                .Select(h => new
                {
                    Handler = h,
                    Order = (h is ISequenceHandler s) ? s.Order : int.MaxValue / 2
                })
                .OrderBy(i=>i.Order)
                .Select(i=>i.Handler)
                .ToArray();
        }
    }
}