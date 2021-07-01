using Pipaslot.Mediator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator
{
    internal static class GenericHelpers
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

        public static Type[] FilterAssignableToRequest(IEnumerable<Type> types)
        {
            var genericRequestType = typeof(IRequest<>);
            return types
                .Where(t => t.IsClass
                        && !t.IsAbstract
                        && !t.IsInterface
                        && t.GetInterfaces()
                            .Any(i => i.IsGenericType
                                    && i.GetGenericTypeDefinition() == genericRequestType)
                )
                .ToArray();
        }

        public static Type[] FilterAssignableToMessage(IEnumerable<Type> types)
        {
            var type = typeof(IMessage);
            return types
                .Where(p => p.IsClass
                            && !p.IsAbstract
                            && !p.IsInterface
                            && p.GetInterfaces().Any(i => i == type))
                .ToArray();
        }
    }
}
