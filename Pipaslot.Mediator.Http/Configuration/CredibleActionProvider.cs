using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using System;
using System.Linq;

namespace Pipaslot.Mediator.Http.Configuration
{
    internal class CredibleActionProvider : ICredibleActionProvider
    {
        private readonly PipelineConfigurator _configurator;

        public CredibleActionProvider(PipelineConfigurator configurator)
        {
            _configurator = configurator;
        }

        public void VerifyCredibility(Type actionType)
        {
            if (_configurator.ActionMarkerAssemblies.Any() && !_configurator.ActionMarkerAssemblies.Contains(actionType.Assembly))
            {
                throw MediatorHttpException.CreateForUnregisteredActionType(actionType);
            }
            if (typeof(IMediatorAction).IsAssignableFrom(actionType))
            {
                return;
            }
            throw MediatorHttpException.CreateForNonContractType(actionType);
        }
    }
}
