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

        protected override async Task HandleMessage<TMessage>(TMessage message, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetMessageHandlers(context.Action.GetType());
            if (handlers.Length == 0)
            {
                throw MediatorException.CreateForNoHandler(context.Action.GetType());
            }
            var sortedHandlers = Sort(handlers);
            foreach (var handler in sortedHandlers)
            {
                await ExecuteMessage(handler, context.Action, context, cancellationToken);
            }
        }
        protected override async Task HandleRequest<TRequest>(TRequest request, MediatorContext context, CancellationToken cancellationToken)
        {
            var handlers = GetRequestHandlers(context.Action.GetType());
            if (handlers.Length == 0)
            {
                throw MediatorException.CreateForNoHandler(context.Action.GetType());
            }
            var sortedHandlers = Sort(handlers);
            foreach (var handler in sortedHandlers)
            {
                await ExecuteRequest(handler, context.Action, context, cancellationToken);
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