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
    public void Add(MediatorContext context)
    {
        var flowMark = new AsyncLocal<bool>
        {
            Value = true //Only the single context will see this flag, for other it will be null
        };
        lock(_lock){
            _stack.Push((flowMark, context));
        }
    }

    /// <summary>
    /// Get actual context which is under execution
    /// </summary>
    public MediatorContext? GetCurrent()
    {
        lock(_lock){
            return GetRelevant().FirstOrDefault();
        }
    }

    /// <summary>
    /// Convert the chain into array. First is the current one, last is the initial (root) action.
    /// </summary>
    /// <returns></returns>
    public MediatorContext[] ToArray()
    {
        lock(_lock){
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