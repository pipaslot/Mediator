using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;

namespace Pipaslot.Mediator
{
    public static class CallStackHelper
    {
        /// <summary>
        /// Extract mediator handler types from execution call stack. This is usefull for monitoring of original handler executing current action
        /// </summary>
        /// <returns>Mediator handler types</returns>
        public static Type[] GetHandlerExecutionStack()
        {
            var stack = new System.Diagnostics.StackTrace();
            var messageHanderType = typeof(IMediatorHandler<>);
            var requestHanderType = typeof(IMediatorHandler<,>);
            return stack.GetFrames()
                .Select(f => f.GetMethod().DeclaringType)
                .Where(t => t != null)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && (i.GetGenericTypeDefinition() == messageHanderType || i.GetGenericTypeDefinition() == requestHanderType)))
                .ToArray();
        }
    }
}
