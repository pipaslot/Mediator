using Pipaslot.Mediator.Abstractions;
using System;
using System.Linq;

namespace Pipaslot.Mediator
{
    internal static class RequestGenericHelpers
    {
        public static Type GetRequestResultType(Type? requestType)
        {
            if (requestType == null)
            {
                throw new ArgumentNullException(nameof(requestType));
            }
            var genericRequestType = typeof(IRequest<>);
            var genericInterface = requestType
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == genericRequestType);
            if (genericInterface == null)
            {
                throw new Exception($"Type {requestType} does not implements {genericRequestType}");
            }
            return genericInterface
                .GetGenericArguments()
                .First();
        }
    }
}
