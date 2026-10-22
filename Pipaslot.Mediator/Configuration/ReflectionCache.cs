using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares.Handlers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Configuration;

/// <summary>
/// Cache of reflection data for registered actions, filled during startup and read on the dispatch hot path.
/// </summary>
/// <remarks>
/// Not meant to be used by applications; use <see cref="Services.IHandlerExistenceChecker"/> to verify registrations.
/// Startup registration is not thread-safe, actions first seen at runtime are cached concurrently.
/// </remarks>
[Obsolete("Infrastructure type that will become internal in 10.0. Resolve IHandlerExistenceChecker from the service provider instead.")]
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

    /// <summary>Result type of the action, or null for an action without a result.</summary>
    public Type? GetRequestResultType(Type actionType)
    {
        if (_startupTimeActions.TryGetValue(actionType, out var cacheItem))
        {
            return cacheItem.ResultType;
        }
        return GetOrAddRuntimeAction(actionType).ResultType;
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public ICollection<Type> GetMessageActionTypes()
    {
        return _startupTimeActions
            .Where(r => !r.Value.HasResultType)
            .Select(r => r.Key)
            .ToArray();
    }

    /// <inheritdoc />
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