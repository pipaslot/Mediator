using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
/// Default <see cref="IMediatorFacade"/> implementation, delegating every member to the registered
/// <see cref="IMediator"/>, <see cref="IMediatorContextAccessor"/> and <see cref="INotificationProvider"/>.
/// </summary>
public class MediatorFacade(IMediator mediator, IMediatorContextAccessor mediatorContextAccessor, INotificationProvider notificationProvider)
    : IMediatorFacade
{
    #region MediatorContextAccessor

    /// <inheritdoc/>
    public MediatorContext? Context => mediatorContextAccessor.Context;

    /// <inheritdoc/>
    public IReadOnlyCollection<MediatorContext> ContextStack => mediatorContextAccessor.ContextStack;

    #endregion

    #region Notification Provider

    /// <inheritdoc/>
    public void AddNotification(Notification notification)
    {
        notificationProvider.Add(notification);
    }

    #endregion

    #region Mediator

    /// <inheritdoc/>
    public Task<IMediatorResponse> Dispatch(IMediatorAction message, CancellationToken cancellationToken = default)
    {
        return mediator.Dispatch(message, cancellationToken);
    }

    /// <inheritdoc/>
    public Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
    {
        return mediator.DispatchUnhandled(message, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
    {
        return mediator.Execute(request, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> request, CancellationToken cancellationToken = default)
    {
        return mediator.ExecuteUnhandled(request, cancellationToken);
    }

    #endregion
}