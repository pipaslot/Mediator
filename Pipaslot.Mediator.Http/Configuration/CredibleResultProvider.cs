using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Http.Configuration
{
    internal class CredibleResultProvider : ICredibleResultProvider
    {
        private readonly PipelineConfigurator _configurator;
        private readonly HashSet<Type> _trustedTypes;
        private HashSet<Type>? _actionResultTypes = null;

        public CredibleResultProvider(PipelineConfigurator configurator, Type[] trustedTypes)
        {
            _configurator = configurator;
            _trustedTypes = new HashSet<Type>(trustedTypes);
        }

        public void VerifyCredibility(Type resultType)
        {
            if (_trustedTypes.Contains(resultType))
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
            throw MediatorHttpException.CreateForUnregisteredResultType(resultType);
        }

        private Type[] GetActionResultTypes()
        {
            return _configurator.GetRequestActionTypes()
                .Select(t => RequestGenericHelpers.GetRequestResultType(t))
                .ToArray();
        }
    }
}
