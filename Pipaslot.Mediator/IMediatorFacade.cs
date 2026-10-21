using Pipaslot.Mediator.Notifications;

namespace Pipaslot.Mediator;

/// <summary>
/// Combines <see cref="IMediator"/>, <see cref="IMediatorContextAccessor"/>, and <see cref="INotificationProvider"/>
/// </summary>
/// <remarks>
/// A convenience for handlers that would otherwise inject all three services. Registered only together with
/// <see cref="IMediatorContextAccessor"/> - <c>AddMediator(addContextAccessor: false)</c> and the default HTTP client
/// setup leave it out, so inject <see cref="IMediator"/> directly where that is the case.
/// </remarks>
public interface IMediatorFacade : IMediator, IMediatorContextAccessor
{
    /// <summary>
    /// Adds <paramref name="notification"/> to the results of the action currently being executed, from where it travels
    /// up to the parent actions and to the caller's <see cref="IMediatorResponse.Results"/>. Called outside any mediator
    /// call, the notification goes straight to the subscribers of <see cref="INotificationReceiver"/>.
    /// </summary>
    /// <remarks>
    /// Adding a notification does not change the outcome of the action - an <see cref="NotificationType.Error"/>
    /// notification is reported among the error messages, but the action still succeeds. To fail an action, use
    /// <see cref="Middlewares.MediatorContextExtensions.AddError(Middlewares.MediatorContext, string, bool)"/> or throw.
    /// </remarks>
    void AddNotification(Notification notification);
}