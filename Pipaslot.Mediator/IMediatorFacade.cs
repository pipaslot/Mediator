using Pipaslot.Mediator.Notifications;

namespace Pipaslot.Mediator;

/// <summary>
/// Combines <see cref="IMediator"/>, <see cref="IMediatorContextAccessor"/>, and <see cref="INotificationProvider"/>
/// </summary>
public interface IMediatorFacade : IMediator, IMediatorContextAccessor
{
    void AddNotification(Notification notification);
}