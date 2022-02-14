using System;
using System.Linq;
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

        protected override async Task HandleMessage(MediatorContext context)
        {
            var handlers = GetMessageHandlers(context.Action.GetType());
            if (handlers.Length > 1)
            {
                throw MediatorException.CreateForDuplicateHandlers(handlers);
            }

            var handler = handlers.FirstOrDefault();
            if (handler == null)
            {
                throw MediatorException.CreateForNoHandler(context.Action.GetType());
            }
            await ExecuteMessage(handler, context);
        }

        protected override async Task HandleRequest(MediatorContext context)
        {
            var handlers = GetRequestHandlers(context.Action.GetType());
            if (handlers.Length > 1)
            {
                throw MediatorException.CreateForDuplicateHandlers(handlers);
            }

            var handler = handlers.FirstOrDefault();
            if (handler == null)
            {
                throw MediatorException.CreateForNoHandler(context.Action.GetType());
            }
            await ExecuteRequest(handler, context);
        }
    }
}