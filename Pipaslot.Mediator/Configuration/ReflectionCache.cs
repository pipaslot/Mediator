using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares.Handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Configuration;

/// <summary>
/// Cache reflection data required on the hot paths. Does not expects concurrency as the setup should happen during application startup. 
/// </summary>
public class ReflectionCache : IActionTypeProvider
{
    /// <summary>
    /// Actions initialized during startup time. We do not need concurrency handling (it is faster). 
    /// </summary>
    private readonly Dictionary<Type, ReflectionCacheItem> _startupTimeActions = new();
    private readonly ConcurrentDictionary<Type, ReflectionCacheItem> _runtimeActions = new();

    internal ReflectionCache AddActions(params Type[] actionType)
    {
        foreach (var action in actionType)
        {
            AddAction(action);
        }

        return this;
    }

    private void AddAction(Type actionType)
    {
        var resultType = GetResultType(actionType);
        var executorType = GetHandlerExecutorGenericType(actionType, resultType);
        _startupTimeActions[actionType] = new ReflectionCacheItem(executorType, resultType);
    }

    internal Type GetHandlerExecutorType(Type actionType)
    {
        if (_startupTimeActions.TryGetValue(actionType, out var cacheItem))
        {
            return cacheItem.ExecutorType;
        }

        return GetOrAddRuntimeAction(actionType).ExecutorType;
    }

    public Type? GetRequestResultType(Type actionType)
    {
        if (_startupTimeActions.TryGetValue(actionType, out var cacheItem))
        {
            return cacheItem.ResultType;
        }
        return GetOrAddRuntimeAction(actionType).ResultType;
    }

    public ICollection<Type> GetActionTypes()
    {
        return _startupTimeActions.Keys;
    }
    

    private ReflectionCacheItem GetOrAddRuntimeAction(Type actionType)
    {
        return _runtimeActions.GetOrAdd(actionType, static type =>
        {
            var resultType = GetResultType(type);
            var executorType = GetHandlerExecutorGenericType(type, resultType);
            return new ReflectionCacheItem(executorType, resultType);
        });
    }

    public ICollection<Type> GetMessageActionTypes()
    {
        return _startupTimeActions
            .Where(r => !r.Value.HasResultType)
            .Select(r => r.Key)
            .ToArray();
    }

    public ICollection<Type> GetRequestActionTypes()
    {
        return _startupTimeActions
            .Where(r => r.Value.HasResultType)
            .Select(r => r.Key)
            .ToArray();
    }

    #region Reflection

    private static Type? GetResultType(Type actionType)
    {
        if (typeof(IMediatorActionProvidingData).IsAssignableFrom(actionType))
        {
            var genericRequestType = typeof(IMediatorAction<>);
            foreach (var iface in actionType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genericRequestType)
                {
                    return iface.GetGenericArguments()[0];
                }
            }
        }

        return null;
    }

    private static Type GetHandlerExecutorGenericType(Type actionType, Type? resultType = null)
    {
        return resultType is not null
            ? typeof(RequestHandlerExecutor<,>).MakeGenericType(actionType, resultType)
            : typeof(MessageHandlerExecutor<>).MakeGenericType(actionType);
    }

    #endregion
}