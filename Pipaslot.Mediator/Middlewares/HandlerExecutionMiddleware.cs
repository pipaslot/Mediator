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
        var executor = context.HasActionReturnValue
            ? GetRequestExecutor(context)
            : GetMessageExecutor(context);

        return executor.Execute(context);
    }

    private HandlerExecutor GetRequestExecutor(MediatorContext context)
    {
        var actionType = context.Action.GetType();
        var executorType = configurator.ReflectionCache.GetHandlerExecutorType(actionType);
        return (HandlerExecutor)context.Services.GetRequiredService(executorType);
    }

    private HandlerExecutor GetMessageExecutor(MediatorContext context)
    {
        var actionType = context.Action.GetType();
        var executorType = configurator.ReflectionCache.GetHandlerExecutorType(actionType);
        return (HandlerExecutor)context.Services.GetRequiredService(executorType);
    }
}