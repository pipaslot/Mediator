namespace Pipaslot.Mediator.Notifications
{
    public static class NotificationProviderExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void Add(this INotificationProvider provider, string content, string source, NotificationType type, bool stopPropagation = false)
        {
            provider.Add(new Notification
            {
                Content = content,
                Source = source,
                Type = type,
                StopPropagation = stopPropagation
            });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddError(this INotificationProvider provider, string content, string source = "", bool stopPropagation = false)
        {
            provider.Add(content, source, NotificationType.Error, stopPropagation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddWarning(this INotificationProvider provider, string content, string source = "", bool stopPropagation = false)
        {
            provider.Add(content, source, NotificationType.Warning, stopPropagation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddInformation(this INotificationProvider provider, string content, string source = "", bool stopPropagation = false)
        {
            provider.Add(content, source, NotificationType.Information, stopPropagation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddSuccess(this INotificationProvider provider, string content, string source = "", bool stopPropagation = false)
        {
            provider.Add(content, source, NotificationType.Success, stopPropagation);
        }
    }
}
