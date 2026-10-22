using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Prevent direct calls for action which are not part of your application API. 
/// Can be used as protection for queries placed in app demilitarized zone (such a actions lacks authentication, authorization or different security checks).
/// </summary>
public class DirectCallProtectionMiddleware(IMediatorContextAccessor contextAccessor) : IMediatorMiddleware
{
    /// <summary>
    /// Throws a <see cref="MediatorException"/> when the action is the outermost mediator call, i.e. not dispatched from
    /// inside another handler or middleware; nested calls continue with the rest of the pipeline.
    /// </summary>
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        // We test for count to be 1 because the actual action is already counted
        if (contextAccessor.ContextStack.Count == 1)
        {
            throw MediatorException.CreateForForbidenDirectCall();
        }

        return next(context);
    }
}