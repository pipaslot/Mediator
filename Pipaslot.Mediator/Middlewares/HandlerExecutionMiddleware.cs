using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares.Handlers;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Execute handlers. No more middlewares will be executed.
/// </summary>
public class HandlerExecutionMiddleware(MediatorConfigurator configurator) : IExecutionMiddleware
{
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        var actionType = context.Action.GetType();
        var executorType = configurator.ReflectionCache.GetHandlerExecutorType(actionType);
        var executor = (HandlerExecutor)context.Services.GetRequiredService(executorType);
        return executor.Execute(context);
    }
}