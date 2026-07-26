using Pipaslot.Mediator.Notifications;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Propagate notification from child to parent context.
/// </summary>
internal class NotificationPropagationMiddleware : IMediatorMiddleware
{
    internal static NotificationPropagationMiddleware Instance { get; } = new ();
    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        await next(context).ConfigureAwait(false);

        if (context.IsNested)
        {
            var parentContext = context.ParentContexts.First();
            var notifications = context.Results
                .Where(r => r is Notification n && !n.StopPropagation)
                .Cast<Notification>();
            foreach (var notification in notifications)
            {
                parentContext.AddForwardedNotification(notification);
            }
        }
    }
}