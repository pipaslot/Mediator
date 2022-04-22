using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Configuration
{
    internal class CredibleActionProvider : ICredibleProvider
    {
        private readonly MediatorConfigurator _configurator;
        private readonly HashSet<Type> _trustedTypes;
        private readonly HashSet<Assembly> _trustedAssemblies;

        public CredibleActionProvider(MediatorConfigurator configurator, IEnumerable<Type> trustedTypes, IEnumerable<Assembly> trustedAssemblies)
        {
            _configurator = configurator;
            _trustedTypes = new HashSet<Type>(trustedTypes);
            _trustedAssemblies = new HashSet<Assembly>(trustedAssemblies);
        }

        public void VerifyCredibility(Type actionType)
        {
            if (_configurator.ActionMarkerAssemblies.Any() 
                && !_configurator.ActionMarkerAssemblies.Contains(actionType.Assembly))
            {
                throw MediatorHttpException.CreateForUnregisteredActionType(actionType);
            }
            if (typeof(IMediatorAction).IsAssignableFrom(actionType))
            {
                return;
            }
            if (_trustedTypes.Contains(actionType)
                || _trustedAssemblies.Contains(actionType.Assembly))
            {
                return;
            }
            var collectionItem = ContractSerializerTypeHelper.GetEnumeratedType(actionType);
            if (collectionItem != null
                && (_trustedTypes.Contains(collectionItem)
                    || _trustedAssemblies.Contains(collectionItem.Assembly)))
            {
                return;
            }
            throw MediatorHttpException.CreateForNonContractType(actionType);
        }
    }
}
