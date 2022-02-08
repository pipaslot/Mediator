namespace Pipaslot.Mediator.Notifications
{
    /// <summary>
    /// Attach notification to action result collection
    /// </summary>
    public interface INotificationProvider
    {
        public void Add(Notification notification);
    }
}
