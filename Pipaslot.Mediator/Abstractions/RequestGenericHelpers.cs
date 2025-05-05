using System;

namespace Pipaslot.Mediator.Abstractions;

public static class RequestGenericHelpers
{
    public static Type GetRequestResultType(Type requestType)
    {
        if (requestType == null)
        {
            throw new ArgumentNullException(nameof(requestType));
        }
    
        var genericRequestType = typeof(IMediatorAction<>);
    
        foreach (var iface in requestType.GetInterfaces())
        {
            if (iface.IsGenericType && iface.GetGenericTypeDefinition() == genericRequestType)
            {
                return iface.GetGenericArguments()[0];
            }
        }
    
        throw new MediatorException($"Type {requestType} does not implement {genericRequestType}");
    }
}