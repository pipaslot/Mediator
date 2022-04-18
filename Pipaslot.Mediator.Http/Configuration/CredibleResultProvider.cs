using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Http.Configuration
{
    internal class CredibleResultProvider : ICredibleResultProvider
    {
        private readonly MediatorConfigurator _configurator;
        private readonly HashSet<Type> _trustedTypes;
        private HashSet<Type>? _actionResultTypes = null;

        public CredibleResultProvider(MediatorConfigurator configurator, Type[] trustedTypes)
        {
            _configurator = configurator;
            _trustedTypes = new HashSet<Type>(trustedTypes);
        }

        public void VerifyCredibility(Type resultType)
        {
            var type = GetEnumeratedType(resultType);
            if (_trustedTypes.Contains(type))
            {
                return;
            }
            if (_actionResultTypes == null)
            {
                _actionResultTypes = new HashSet<Type>(GetActionResultTypes());
            }
            if (_actionResultTypes.Contains(type))
            {
                return;
            }
            throw MediatorHttpException.CreateForUnregisteredResultType(type);
        }

        private Type[] GetActionResultTypes()
        {
            var requestInterface = typeof(IMediatorActionProvidingData);
            return _configurator.ActionMarkerAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => requestInterface.IsAssignableFrom(t))
                .Select(t => RequestGenericHelpers.GetRequestResultType(t))
                .ToArray();
        }

        private Type GetEnumeratedType(Type type)
        {
            if (type.IsArray)
            {
                var elType = type.GetElementType();
                if (null != elType)
                {
                    return elType;
                }
            }

            if (ContractSerializerTypeHelper.IsEnumerable(type)) {
                var elTypes = type.GetGenericArguments();
                if (elTypes.Length > 0)
                {
                    return elTypes[0];
                }
            }

            return type;
        }
    }
}
