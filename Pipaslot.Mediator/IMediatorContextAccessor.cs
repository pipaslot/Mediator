using Pipaslot.Mediator.Middlewares;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator;

/// <summary>
/// Gives code outside the middleware signature - typically a handler, or a service called from one - access to the
/// <see cref="MediatorContext"/> of the action currently being executed and of its parent actions.
/// </summary>
/// <remarks>
/// Registered as a scoped service by <c>AddMediator</c> unless it is called with <c>addContextAccessor: false</c>; on the
/// HTTP client (<c>AddMediatorClient</c>) it is opt-in via the <c>AddContextAccessor</c> option. A middleware does not
/// need it - it receives the context as a parameter. <see cref="IMediatorFacade"/> exposes the same members together with
/// <see cref="IMediator"/>.
/// </remarks>
public interface IMediatorContextAccessor
{
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

/// <summary>
/// Navigation helpers over <see cref="IMediatorContextAccessor.ContextStack"/> for nested mediator calls.
/// </summary>
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

    /// <summary>
    /// Get feature from root context
    /// </summary>
    public static TFeature? GetRootContextFeature<TFeature>(this IMediatorContextAccessor accessor)
    {
        var context = accessor.GetRootContext();
        if (context is not null)
        {
            return context.Features.Get<TFeature>();
        }

        return default;
    }

    /// <summary>
    /// Sets <paramref name="feature"/> on the features of the root context, so it is visible to every action of the current
    /// call tree and to the code that started it.
    /// </summary>
    /// <returns>False when no action is being executed - there is no root context and the feature is dropped.</returns>
    public static bool SetRootContextFeature<TFeature>(this IMediatorContextAccessor accessor, TFeature feature)
    {
        var context = accessor.GetRootContext();
        if (context is not null)
        {
            context.Features.Set(feature);
            return true;
        }

        return false;
    }
}