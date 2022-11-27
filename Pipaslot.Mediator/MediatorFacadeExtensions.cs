using Pipaslot.Mediator.Notifications;

namespace Pipaslot.Mediator
{
    public static class MediatorFacadeExtensions
    {
        public static void AddNotification(this IMediatorFacade provider, string content, string source, NotificationType type)
        {
            provider.AddNotification(new Notification
            {
                Content = content,
                Source = source,
                Type = type
            });
        }

        public static void AddNotification(this IMediatorFacade provider, string content, NotificationType type, string? source = null)
        {
            provider.AddNotification(new Notification
            {
                Content = content,
                Source = source ?? string.Empty,
                Type = type
            });
        }

        public static void AddErrorNotification(this IMediatorFacade facade, string content, string source = "")
        {
            facade.AddNotification(content, source, NotificationType.Error);
        }

        public static void AddWarningNotification(this IMediatorFacade facade, string content, string source = "")
        {
            facade.AddNotification(content, source, NotificationType.Warning);
        }

        public static void AddInformationNotification(this IMediatorFacade facade, string content, string source = "")
        {
            facade.AddNotification(content, source, NotificationType.Information);
        }

        public static void AddSuccessNotification(this IMediatorFacade facade, string content, string source = "")
        {
            facade.AddNotification(content, source, NotificationType.Success);
        }
    }
}
