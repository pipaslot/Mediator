using Pipaslot.Mediator.Notifications;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Propagate notification from child to parent context.
    /// </summary>
    internal class NotificationPropagationMiddleware : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            await next(context).ConfigureAwait(false);

            var parentContext = context.ParentContexts.FirstOrDefault();
            if(parentContext is not null){
                var notifications = context.Results.Where(r => r is Notification n && !n.StopPropagation);
                parentContext.AddResults(notifications);
            }
        }
    }
}
