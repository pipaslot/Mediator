using Pipaslot.Mediator.Middlewares;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Pipaslot.Mediator;

/// <summary>
/// Reflect chaining(mediator call recursion) of action. Track the current action and their parents.
/// </summary>
internal class ContextFlow
{
    private readonly Stack<(AsyncLocal<bool> FlowMark, MediatorContext Context)> _stack = new();
    private readonly object _lock = new();

    /// <summary>
    /// Add new context to the flow, representing action under execution
    /// </summary>
    /// <param name="context"></param>
    public int Add(MediatorContext context)
    {
        var flowMark = new AsyncLocal<bool>
        {
            // For tracking path of the current thread. 
            // Will be true for the current threads and its child/tested threads.
            // For concurrent branches it will be null
            Value = true
        };
        lock (_lock)
        {
            var count = 0;
            foreach (var item in _stack)
            {
                if (item.FlowMark is not null && item.FlowMark.Value)
                {
                    count++;
                }
            }
            _stack.Push((flowMark, context));

            return count + 1;
        }
    }

    /// <summary>
    /// Get actual context which is under execution
    /// </summary>
    public MediatorContext? GetCurrent()
    {
        lock (_lock)
        {
            return GetRelevant().FirstOrDefault();
        }
    }

    /// <summary>
    /// Convert the chain into array. First is the current one, last is the initial (root) action.
    /// </summary>
    /// <returns></returns>
    public MediatorContext[] ToArray()
    {
        lock (_lock)
        {
            return GetRelevant().ToArray();
        }
    }

    /// <summary>
    /// Return only those contexts related to the actual flow
    /// </summary>
    /// <returns></returns>
    private IEnumerable<MediatorContext> GetRelevant()
    {
        return _stack
            .Where(p => p.FlowMark is not null && p.FlowMark.Value)
            .Select(p => p.Context);
    }
}