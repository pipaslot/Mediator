using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator
{
    public interface IMediatorContextAccessor
    {
        /// <summary>
        /// Returns context of actual executed action
        /// </summary>
        [Obsolete("Use Context instead.")]
        MediatorContext? MediatorContext { get; }

        /// <summary>
        /// Returns context of actual executed action.
        /// WARNING: If you need to access the context in the handlers, make sure that you access it before first awaited method call.
        /// In case of concurrent mediator calls you may get different context depending by the raise condition and thread ordering caused by async/await state machine.
        /// </summary>
        MediatorContext? Context { get; }

        /// <summary>
        /// All action context executed recursively
        /// First is the actual action
        /// Last is the root action
        /// </summary>
        IReadOnlyCollection<MediatorContext> ContextStack { get; }
    }

    public static class MediatorContextAccessorExtensions
    {
        /// <summary>
        /// Return the top level context initiating any further lower level actions.
        /// </summary>
        public static MediatorContext? GetRootContext(this IMediatorContextAccessor accessor)
        {
            return accessor.ContextStack.LastOrDefault();
        }
        /// <summary>
        /// Parent action contexts. 
        /// Will be empty if current action is executed independently. 
        /// Will contain parent contexts of actions which executed current action as nested call. 
        /// The last member is always the root action.
        /// </summary>
        public static MediatorContext[] GetParentContexts(this IMediatorContextAccessor accessor)
        {
            return accessor.ContextStack.Skip(1).ToArray();
        }
    }
}
