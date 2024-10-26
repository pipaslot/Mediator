﻿using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pipaslot.Mediator.Http.Configuration;

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

        var collectionItem = ContractSerializerTypeHelper.GetEnumeratedType(resultType);
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

    private IEnumerable<Type> GetActionResultTypes()
    {
        return _configurator.GetRequestActionTypes()
            .Select(t => RequestGenericHelpers.GetRequestResultType(t))
            .Select(t => ContractSerializerTypeHelper.GetEnumeratedType(t) ?? t);
    }
}