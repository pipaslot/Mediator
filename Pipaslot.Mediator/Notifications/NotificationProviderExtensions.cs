namespace Pipaslot.Mediator.Notifications
{
    public static class NotificationProviderExtensions
    {
        public static void Add(this INotificationProvider provider, string content, string source, NotificationType type)
        {
            provider.Add(new Notification
            {
                Content = content,
                Source = source,
                Type = type
            });
        }
        public static void AddError(this INotificationProvider provider, string content, string source = "")
        {
            provider.Add(content, source, NotificationType.Error);
        }

        public static void AddWarning(this INotificationProvider provider, string content, string source = "")
        {
            provider.Add(content, source, NotificationType.Warning);
        }

        public static void AddInformation(this INotificationProvider provider, string content, string source = "")
        {
            provider.Add(content, source, NotificationType.Information);
        }

        public static void AddSuccess(this INotificationProvider provider, string content, string source = "")
        {
            provider.Add(content, source, NotificationType.Success);
        }
    }
}
