using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares.Handlers;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Execute handlers. No more middlewares will be executed.
/// </summary>
public class HandlerExecutionMiddleware : IExecutionMiddleware
{
    private static readonly ConcurrentDictionary<Type, (Type ResultType, Type ExecutorType)> _executorInfoCache = new();

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
        var (resultType, executorType) = _executorInfoCache.GetOrAdd(actionType, static type =>
        {
            var resultType = RequestGenericHelpers.GetRequestResultType(type);
            var executorType = typeof(RequestHandlerExecutor<,>).MakeGenericType(type, resultType);
            return (resultType, executorType);
        });

        return (HandlerExecutor)context.Services.GetRequiredService(executorType);
    }

    private static HandlerExecutor GetMessageExecutor(MediatorContext context)
    {
        var actionType = context.Action.GetType();
        var executorType = _executorInfoCache.GetOrAdd(actionType, static type =>
        {
            var executor = typeof(MessageHandlerExecutor<>).MakeGenericType(type);
            return (null!, executor); // resultType unused for message handlers
        }).ExecutorType;

        return (HandlerExecutor)context.Services.GetRequiredService(executorType);
    }
}