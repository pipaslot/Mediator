using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator;

/// <summary>
/// Payload of <see cref="Middlewares.ActionEventsMiddleware.ActionCompleted"/>, raised when an action leaves the pipeline
/// of a mediator registered with <c>UseActionEvents()</c> - after success, failure and an exception alike.
/// </summary>
/// <param name="action">The action that just left the pipeline.</param>
/// <param name="runningActions">Snapshot of the actions still in progress, without <paramref name="action"/>.</param>
public class ActionCompletedEventArgs(IMediatorAction action, IReadOnlyCollection<IMediatorAction> runningActions)
    : EventArgs
{
    /// <summary>
    /// Completed action
    /// </summary>
    public IMediatorAction Action { get; } = action;

    /// <summary>
    /// Actions still in progress, no longer including <see cref="Action"/>. A snapshot taken when the event was raised;
    /// empty when this was the last running action.
    /// </summary>
    public IReadOnlyCollection<IMediatorAction> RunningActions { get; } = runningActions;
}