﻿using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Notifications;

public class Notification : IEquatable<Notification?>
{
    public DateTime Time { get; set; } = DateTime.Now;

    /// <summary>
    /// Name of action for which the notification was raised.
    /// Can be also used as title.
    /// </summary>
    public string Source { get; set; } = "";

    public string Content { get; set; } = "";

    /// <summary>
    /// If True, then the notification will NOT be propagated from nested mediator calls to the callers (aggregated in the root action call). 
    /// </summary>
    public bool StopPropagation { get; set; }

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
               Type == other.Type;
    }

    public override int GetHashCode()
    {
        var hashCode = 1045479575;
        hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Source);
        hashCode = (hashCode * -1521134295) + EqualityComparer<string>.Default.GetHashCode(Content);
        hashCode = (hashCode * -1521134295) + Type.GetHashCode();
        return hashCode;
    }

    internal static Notification Error(string content, IMediatorAction action, bool stopPropagation = false)
    {
        return new Notification
        {
            Type = NotificationType.Error, Content = content, Source = action.GetActionName(), StopPropagation = stopPropagation
        };
    }

    internal static Notification Error(string content, string source = "", bool stopPropagation = false)
    {
        return new Notification { Type = NotificationType.Error, Content = content, Source = source, StopPropagation = stopPropagation };
    }
}