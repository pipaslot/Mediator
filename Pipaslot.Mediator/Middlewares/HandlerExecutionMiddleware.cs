using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares.Handlers;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Execute handlers. No more middlewares will be executed.
/// </summary>
public class HandlerExecutionMiddleware : IExecutionMiddleware
{
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        var executor = context.HasActionReturnValue
            ? GetRequestExecutor(context)
            : GetMessageExecutor(context);
        return executor.Execute(context);
    }

    private static HandlerExecutor GetRequestExecutor(MediatorContext context)
    {
        var actionType = context.Action.GetType();
        var resultType = RequestGenericHelpers.GetRequestResultType(actionType);
        var executorType = typeof(RequestHandlerExecutor<,>).MakeGenericType(actionType, resultType);
        return (HandlerExecutor)context.Services.GetRequiredService(executorType); //TODO cache in the context
    }

    private static HandlerExecutor GetMessageExecutor(MediatorContext context)
    {
        var executorType = typeof(MessageHandlerExecutor<>).MakeGenericType(context.Action.GetType());
        return (HandlerExecutor)context.Services.GetRequiredService(executorType); //TODO cache in the context
    }
}