using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator;

/// <summary>
/// Payload of <see cref="Middlewares.ActionEventsMiddleware.ActionStarted"/>, raised when an action enters the pipeline
/// of a mediator registered with <c>UseActionEvents()</c>.
/// </summary>
/// <param name="action">The action that just entered the pipeline.</param>
/// <param name="runningActions">Snapshot of all actions in progress, including <paramref name="action"/>.</param>
public class ActionStartedEventArgs(IMediatorAction action, IReadOnlyCollection<IMediatorAction> runningActions)
    : EventArgs
{
    /// <summary>
    /// Started action
    /// </summary>
    public IMediatorAction Action { get; } = action;

    /// <summary>
    /// Actions currently in progress, including <see cref="Action"/> itself. A snapshot taken when the event was raised.
    /// </summary>
    public IReadOnlyCollection<IMediatorAction> RunningActions { get; } = runningActions;
}