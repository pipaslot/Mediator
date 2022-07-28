using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Notifications
{
    public class Notification : IEquatable<Notification?>
    {
        public DateTime Time { get; set; } = DateTime.Now;
        public string Source { get; set; } = "";
        public string Content { get; set; } = "";
        public NotificationType Type { get; set; } = NotificationType.Information;

        public override bool Equals(object? obj)
        {
            return Equals(obj as Notification);
        }

        public bool Equals(Notification? other)
        {
            return other != null &&
                   Source == other.Source &&
                   Content == other.Content &&
                   Type.ToUnified() == other.Type.ToUnified();
        }

        public override int GetHashCode()
        {
            var hashCode = 1045479575;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Source);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Content);
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            return hashCode;
        }

        internal static Notification ActionError(string content, IMediatorAction action)
        {
            return new Notification
            {
                Type = NotificationType.ActionError,
                Content = content,
                Source = action?.GetType()?.ToString() ?? ""
            };
        }

        internal static Notification Error(string content, IMediatorAction action)
        {
            return new Notification
            {
                Type = NotificationType.Error,
                Content = content,
                Source = action?.GetType()?.ToString() ?? ""
            };
        }

        internal static Notification Error(string content, string source = "")
        {
            return new Notification
            {
                Type = NotificationType.Error,
                Content = content,
                Source = source
            };
        }
    }
}
