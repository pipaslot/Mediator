namespace Pipaslot.Mediator.Notifications;

/// <summary>
/// Attach notification to action result collection
/// </summary>
/// TODO: The notification propagation between provider, receiver and receiver middleware is not completely clear. We need to prepare better approach
public interface INotificationProvider
{
    public void Add(Notification notification);
}