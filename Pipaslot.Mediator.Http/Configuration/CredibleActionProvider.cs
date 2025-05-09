using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Configuration;

/// <param name="configurator"></param>
/// <param name="trustedTypes">Credible types coming from consumer app configuration</param>
/// <param name="trustedAssemblies">Credible assemblies coming from consumer app configuration</param>
internal class CredibleActionProvider(MediatorConfigurator configurator, IEnumerable<Type> trustedTypes, IEnumerable<Assembly> trustedAssemblies)
    : ICredibleProvider
{
    private readonly HashSet<Type> _trustedTypes = [..trustedTypes];
    private readonly HashSet<Assembly> _trustedAssemblies = [..trustedAssemblies];

    public void VerifyCredibility(Type actionType)
    {
        if (configurator.TrustedAssemblies.Any() && !configurator.TrustedAssemblies.Contains(actionType.Assembly))
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