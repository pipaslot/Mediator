using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pipaslot.Mediator;

/// <summary>
/// Scoped service which uses AsyncLocal for thread isolation for the context stack
/// </summary>
internal class MediatorContextAccessor(IServiceProvider serviceProvider) : IMediatorContextAccessor, INotificationProvider
{
    private static readonly AsyncLocal<ContextFlow> _asyncLocal = new();

    public MediatorContext? Context => _asyncLocal.Value?.GetCurrent();

    public IReadOnlyCollection<MediatorContext> ContextStack => _asyncLocal.Value?.ToArray() ?? Array.Empty<MediatorContext>();

    /// <returns>Amount of contexts already stored on the stack</returns>
    public int Push(MediatorContext context)
    {
        var existing = _asyncLocal.Value;
        if (existing is null)
        {
            var flow = new ContextFlow();
            flow.Add(context);
            _asyncLocal.Value = flow;
            return 1;
        }

        return existing.Add(context);
    }

    public void Add(Notification notification)
    {
        if (Context is null)
        {
            // Notification provider is called independently of the mediator
            var messageReceiver = serviceProvider.GetService<NotificationReceiverMiddleware>();
            messageReceiver?.SendNotifications(notification);
        }
        else
        {
            Context.AddResult(notification);
        }
    }
}