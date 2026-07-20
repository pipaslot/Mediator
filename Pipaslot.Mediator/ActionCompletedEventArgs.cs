using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator;

public class ActionCompletedEventArgs(IMediatorAction action, IReadOnlyCollection<IMediatorAction> runningActions)
    : EventArgs
{
    /// <summary>
    /// Completed action
    /// </summary>
    public IMediatorAction Action { get; } = action;

    /// <summary>
    /// Actions currently in progress
    /// </summary>
    public IReadOnlyCollection<IMediatorAction> RunningActions { get; } = runningActions;
}