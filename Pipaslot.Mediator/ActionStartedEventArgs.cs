using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator;

public class ActionStartedEventArgs(IMediatorAction action, IReadOnlyCollection<IMediatorAction> runningActions)
    : EventArgs
{
    /// <summary>
    /// Started action
    /// </summary>
    public IMediatorAction Action { get; } = action;

    /// <summary>
    /// Actions currently in progress
    /// </summary>
    public IReadOnlyCollection<IMediatorAction> RunningActions { get; } = runningActions;
}