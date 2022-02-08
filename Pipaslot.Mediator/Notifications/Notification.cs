using System;

namespace Pipaslot.Mediator.Notifications
{
    public class Notification
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string Source { get; set; } = "";
        public string Content { get; set; } = "";
        public NotificationType Type { get; set; } = NotificationType.Information;
    }
}
