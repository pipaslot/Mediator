using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator
{
    public class ActionStartedEventArgs : EventArgs
    {
        public ActionStartedEventArgs(IMediatorAction action, IReadOnlyCollection<IMediatorAction> runningActions)
        {
            Action = action;
            RunningActions = runningActions;
        }

        /// <summary>
        /// Started action
        /// </summary>
        public IMediatorAction Action { get; }

        /// <summary>
        /// Actions currently in progress
        /// </summary>
        public IReadOnlyCollection<IMediatorAction> RunningActions { get; }
    }
}
