using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Configuration
{
    internal class CredibleResultProvider : ICredibleProvider
    {
        private readonly MediatorConfigurator _configurator;
        private readonly HashSet<Type> _trustedTypes;
        private readonly HashSet<Assembly> _trustedAssemblies;
        private HashSet<Type>? _actionResultTypes = null;

        public CredibleResultProvider(MediatorConfigurator configurator, IEnumerable<Type> trustedTypes, IEnumerable<Assembly> trustedAssemblies)
        {
            _configurator = configurator;
            _trustedTypes = new HashSet<Type>(trustedTypes);
            _trustedAssemblies = new HashSet<Assembly>(trustedAssemblies);
        }

        public void VerifyCredibility(Type resultType)
        {
            if (_trustedTypes.Contains(resultType)
                || _trustedAssemblies.Contains(resultType.Assembly))
            {
                return;
            }
            var collectionItem = GetEnumeratedType(resultType);
            if (collectionItem != null
                && (_trustedTypes.Contains(collectionItem)
                    || _trustedAssemblies.Contains(collectionItem.Assembly)))
            {
                return;
            }
            if (_actionResultTypes == null)
            {
                _actionResultTypes = new HashSet<Type>(GetActionResultTypes());
            }
            if (_actionResultTypes.Contains(resultType))
            {
                return;
            }
            if (collectionItem != null
                && _actionResultTypes.Contains(collectionItem))
            {
                return;
            }
            throw MediatorHttpException.CreateForUnregisteredResultType(collectionItem ?? resultType);
        }

        private Type[] GetActionResultTypes()
        {
            var requestInterface = typeof(IMediatorActionProvidingData);
            return _configurator.ActionMarkerAssemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => requestInterface.IsAssignableFrom(t))
                .Select(t => RequestGenericHelpers.GetRequestResultType(t))
                .Select(t => GetEnumeratedType(t) ?? t)
                .ToArray();
        }

        private Type? GetEnumeratedType(Type type)
        {
            if (type.IsArray)
            {
                var elType = type.GetElementType();
                if (null != elType)
                {
                    return elType;
                }
            }

            if (ContractSerializerTypeHelper.IsEnumerable(type))
            {
                var elTypes = type.GetGenericArguments();
                if (elTypes.Length > 0)
                {
                    return elTypes[0];
                }
            }
            return null;
        }
    }
}
