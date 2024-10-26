using System;
using System.Linq;

namespace Pipaslot.Mediator.Abstractions;

public static class RequestGenericHelpers
{
    public static Type GetRequestResultType(Type? requestType)
    {
        if (requestType == null)
        {
            throw new ArgumentNullException(nameof(requestType));
        }

        var genericRequestType = typeof(IMediatorAction<>);
        var genericInterface = requestType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericRequestType);
        if (genericInterface == null)
        {
            throw new MediatorException($"Type {requestType} does not implements {genericRequestType}");
        }

        return genericInterface
            .GetGenericArguments()
            .First();
    }
}