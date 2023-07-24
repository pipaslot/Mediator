using Pipaslot.Mediator.Notifications;

namespace Pipaslot.Mediator
{
    public static class MediatorFacadeExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddNotification(this IMediatorFacade facade, string content, string source, NotificationType type, bool stopPropagation = false)
        {
            facade.AddNotification(new Notification
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
        /// <param name="facade"></param>
        /// <param name="content"></param>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddNotification(this IMediatorFacade facade, string content, NotificationType type, string? source = null, bool stopPropagation = false)
        {
            facade.AddNotification(new Notification
            {
                Content = content,
                Source = source ?? string.Empty,
                Type = type,
                StopPropagation = stopPropagation
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddErrorNotification(this IMediatorFacade facade, string content, string source = "", bool stopPropagation = false)
        {
            facade.AddNotification(content, source, NotificationType.Error, stopPropagation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddWarningNotification(this IMediatorFacade facade, string content, string source = "", bool stopPropagation = false)
        {
            facade.AddNotification(content, source, NotificationType.Warning, stopPropagation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddInformationNotification(this IMediatorFacade facade, string content, string source = "", bool stopPropagation = false)
        {
            facade.AddNotification(content, source, NotificationType.Information, stopPropagation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="facade"></param>
        /// <param name="content"></param>
        /// <param name="source"></param>
        /// <param name="stopPropagation"><inheritdoc cref="Notification.StopPropagation" path="/summary"/></param>
        public static void AddSuccessNotification(this IMediatorFacade facade, string content, string source = "", bool stopPropagation = false)
        {
            facade.AddNotification(content, source, NotificationType.Success, stopPropagation);
        }
    }
}
