﻿using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using System;

namespace Pipaslot.Mediator.Http
{
    public class MediatorHttpException : Exception
    {
        public MediatorHttpException(string message) : base(message)
        {
        }

        internal static MediatorHttpException CreateForEmptyBody()
        {
            return new MediatorHttpException("Request body has empty body. JSON was expected.");
        }

        internal static MediatorHttpException CreateForUnregisteredService(Type serviceType)
        {
            return new MediatorHttpException($"Service {serviceType.FullName} was not registered in service collection");
        }

        internal static MediatorHttpException CreateForUnparsedContract()
        {
            return new MediatorHttpException($"Can not parse contract object from request body");
        }

        internal static MediatorHttpException CreateForUnrecognizedType(string objectName)
        {
            return new MediatorHttpException($"Can not recognize type {objectName}");
        }

        internal static MediatorHttpException CreateForUnregisteredType(Type queryType)
        {
            return new MediatorHttpException($"Received contract type {queryType.FullName} does not belongs into trusted Marker Assemblies registered in mediator configuration via {nameof(IPipelineConfigurator.AddHandlersFromAssembly)} or {nameof(IPipelineConfigurator.AddHandlersFromAssemblyOf)}");
        }

        internal static MediatorHttpException CreateForNonContractType(Type queryType)
        {
            return new MediatorHttpException($"Received contract type {queryType.FullName} does not implements interface {nameof(IMediatorAction)}");
        }
    }
}