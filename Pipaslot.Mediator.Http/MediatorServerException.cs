using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using System;

namespace Pipaslot.Mediator.Http
{
    public class MediatorServerException : Exception
    {
        public MediatorServerException(string message) : base(message)
        {
        }

        internal static MediatorServerException CreateForEmptyBody()
        {
            return new MediatorServerException("Request body has empty body. JSON was expected.");
        }

        internal static MediatorServerException CreateForUnregisteredService(Type serviceType)
        {
            return new MediatorServerException($"Service {serviceType.FullName} was not registered in service collection");
        }

        internal static MediatorServerException CreateForUnparsedContract()
        {
            return new MediatorServerException($"Can not parse contract object from request body");
        }

        internal static MediatorServerException CreateForUnrecognizedType(string objectName)
        {
            return new MediatorServerException($"Can not recognize type {objectName}");
        }

        internal static MediatorServerException CreateForUnregisteredType(Type queryType)
        {
            return new MediatorServerException($"Received contract type {queryType.FullName} does not belongs into trusted Marker Assemblies registered in mediator configuration via {nameof(IPipelineConfigurator.AddHandlersFromAssembly)} or {nameof(IPipelineConfigurator.AddHandlersFromAssemblyOf)}");
        }

        internal static MediatorServerException CreateForNonContractType(Type queryType)
        {
            return new MediatorServerException($"Received contract type {queryType.FullName} does not implements interface {nameof(IMediatorAction)}");
        }
    }
}
