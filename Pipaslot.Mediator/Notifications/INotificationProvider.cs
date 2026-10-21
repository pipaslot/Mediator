namespace Pipaslot.Mediator.Notifications;

/// <summary>
/// Attach notification to action result collection
/// </summary>
/// <remarks>
/// Inject it into a handler or service to report a message (success, information, warning or error) to the caller of the
/// action. With the default registration (<c>addContextAccessor: true</c>) the notification is added to
/// <see cref="Middlewares.MediatorContext.Results"/> of the current context inside an action execution, and delivered
/// straight to <see cref="INotificationReceiver"/> subscribers outside of any execution. With <c>addContextAccessor: false</c>
/// it is always delivered straight to the subscribers and never reaches <see cref="Middlewares.MediatorContext.Results"/>.
/// Adding a notification never changes <see cref="Middlewares.MediatorContext.Status"/>, not even for
/// <see cref="NotificationType.Error"/> - to fail the action use <see cref="Middlewares.MediatorContextExtensions.AddError(Middlewares.MediatorContext, string, bool)"/>
/// or throw. Prefer <see cref="IMediatorFacade"/> when you also need <see cref="IMediator"/>.
/// </remarks>
public interface INotificationProvider
{
    /// <summary>
    /// With <c>addContextAccessor: true</c> (default) adds the notification to the action currently being executed - an equal
    /// notification already present in the results is not added twice - or passes it to <see cref="INotificationReceiver"/>
    /// subscribers when no action is running. With <c>addContextAccessor: false</c> always passes it to the subscribers.
    /// Does not change <see cref="Middlewares.MediatorContext.Status"/>.
    /// </summary>
    /// <param name="notification">The notification to report</param>
    public void Add(Notification notification);
}