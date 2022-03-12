using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests.Notifications
{
    public class NotificationTests
    {
        [Fact]
        public void TwoSameNotificationsAreEqualEvenIfDifferentTime()
        {
            var notification = Create();
            notification.Time = DateTime.Now.AddHours(1);
            Compare(notification, true);
        }

        [Fact]
        public void NotificationWithDifferentType_AreNotEqual()
        {
            var notification = Create();
            notification.Type = NotificationType.Warning;
            Compare(notification, false);
        }

        [Fact]
        public void NotificationWithDifferentSource_AreNotEqual()
        {
            var notification = Create();
            notification.Source = "haha";
            Compare(notification, false);
        }

        [Fact]
        public void NotificationWithDifferentContent_AreNotEqual()
        {
            var notification = Create();
            notification.Content = "haha";
            Compare(notification, false);
        }

        private Notification Create()
        {
            return new Notification
            {
                Content = "Text",
                Source = "Header",
                Time = DateTime.Now,
                Type = NotificationType.Success
            };
        }

        private void Compare(Notification notification, bool equal)
        {
            var n = Create();
            if (equal)
            {
                Assert.Equal(n, notification);
                Assert.Equal(n.GetHashCode(), notification.GetHashCode());
            }
            else
            {
                Assert.NotEqual(n, notification);
                Assert.NotEqual(n.GetHashCode(), notification.GetHashCode());
            }
        }
    }
}
