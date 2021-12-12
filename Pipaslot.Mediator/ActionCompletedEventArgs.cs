using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pipaslot.Mediator
{
    public class ActionCompletedEventArgs : EventArgs
    {
        public ActionCompletedEventArgs(IMediatorAction action, IReadOnlyCollection<IMediatorAction> runningActions)
        {
            Action = action;
            RunningActions = runningActions;
        }

        /// <summary>
        /// Completed action
        /// </summary>
        public IMediatorAction Action { get; }

        /// <summary>
        /// Actions currently in progress
        /// </summary>
        public IReadOnlyCollection<IMediatorAction> RunningActions { get; }
    }
}
