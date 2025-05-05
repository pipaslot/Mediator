using System;
using System.Collections.Concurrent;

namespace Pipaslot.Mediator.Abstractions;

public static class RequestGenericHelpers
{
    private static readonly ConcurrentDictionary<Type, Type> _cache = new();

    public static Type GetRequestResultType(Type requestType)
    {
        if (requestType == null) throw new ArgumentNullException(nameof(requestType));

        return _cache.GetOrAdd(requestType, static type =>
        {
            var genericRequestType = typeof(IMediatorAction<>);
            foreach (var iface in type.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genericRequestType)
                {
                    return iface.GetGenericArguments()[0];
                }
            }

            throw new MediatorException($"Type {type} does not implement {genericRequestType}");
        });
    }
}