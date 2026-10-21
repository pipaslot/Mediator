using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Notifications;

/// <summary>
/// Message reported to the caller of an action next to its result, e.g. for display in a notification component.
/// It is stored in <see cref="Middlewares.MediatorContext.Results"/> and travels in the response, also over HTTP.
/// </summary>
/// <remarks>
/// Create it through <see cref="INotificationProvider"/> extensions rather than by hand. Two notifications are equal when
/// <see cref="Source"/>, <see cref="Content"/> and <see cref="Type"/> match - <see cref="Time"/> and
/// <see cref="StopPropagation"/> are ignored - and an equal one is not added to the results twice.
/// </remarks>
public class Notification : IEquatable<Notification?>
{
    /// <summary>
    /// Moment the notification was created. Notifications are delivered to <see cref="INotificationReceiver"/> in this order.
    /// </summary>
    public DateTime Time { get; set; } = DateTime.Now;

    /// <summary>
    /// Name of action for which the notification was raised.
    /// Can be also used as title.
    /// </summary>
    public string Source { get; set; } = "";

    /// <summary>
    /// Message text shown to the user. For <see cref="NotificationType.Error"/> it is what
    /// <see cref="MediatorResponseExtensions.GetErrorMessages(IMediatorResponse)"/> returns.
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// If True, then the notification will NOT be propagated from nested mediator calls to the callers (aggregated in the root action call). 
    /// </summary>
    public bool StopPropagation { get; set; }

    /// <summary>
    /// Severity of the notification. Only <see cref="NotificationType.Error"/> is interpreted by the library;
    /// the other values are for the consuming UI.
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Information;

    /// <inheritdoc cref="Equals(Notification?)"/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as Notification);
    }

    /// <summary>
    /// Compares <see cref="Source"/>, <see cref="Content"/> and <see cref="Type"/> only.
    /// </summary>
    public bool Equals(Notification? other)
    {
        return other != null &&
               Source == other.Source &&
               Content == other.Content &&
               Type == other.Type;
    }

    /// <inheritdoc cref="Equals(Notification?)"/>
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