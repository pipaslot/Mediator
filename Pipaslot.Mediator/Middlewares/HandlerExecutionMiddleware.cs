using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Execute handlers. No more middlewares will be executed.
/// </summary>
public class HandlerExecutionMiddleware : IExecutionMiddleware
{
    /// <summary>
    /// Resolves and runs the handler(s) registered for the action. Never calls <c>next</c>, so middlewares registered after it
    /// do not run.
    /// </summary>
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        var executor = context.GetHandlerExecutor();
        return executor.Execute(context);
    }
}