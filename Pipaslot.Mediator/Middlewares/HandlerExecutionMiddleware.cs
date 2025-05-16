using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Execute handlers. No more middlewares will be executed.
/// </summary>
public class HandlerExecutionMiddleware : IExecutionMiddleware
{
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        var executor = context.GetHandlerExecutor();
        return executor.Execute(context);
    }
}