using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using System;

namespace Pipaslot.Mediator.Http;

public class MediatorHttpException(string message, Exception? innerException = null) : Exception(message, innerException)
{
    internal static MediatorHttpException CreateForInvalidRequest(Exception? innerException = null)
    {
        return new MediatorHttpException($"Request body is not valid JSON contract expected by the mediator.", innerException);
    }

    internal static MediatorHttpException CreateForInvalidResponse(string? content, Exception? innerException = null)
    {
        return new MediatorHttpException($"Response body '{content}' is not valid JSON contract expected by the mediator.", innerException);
    }

    internal static MediatorHttpException CreateForUnregisteredService(Type serviceType)
    {
        return new MediatorHttpException($"Service {serviceType.FullName} was not registered in service collection.");
    }

    internal static MediatorHttpException CreateForUnrecognizedType(string objectName)
    {
        return new MediatorHttpException($"Can not recognize type {objectName}.");
    }

    internal static MediatorHttpException CreateForUnregisteredActionType(Type queryType)
    {
        return new MediatorHttpException(
            $"Received action type {queryType.FullName} does not belongs into trusted Marker Assemblies registered in mediator configuration via {nameof(IMediatorConfigurator.AddActionsFromAssembly)} or {nameof(IMediatorConfigurator.AddActionsFromAssemblyOf)}.");
    }

    internal static MediatorHttpException CreateForUnregisteredResultType(Type resultType)
    {
        return new MediatorHttpException(
            $"Received result type {resultType.FullName} does not belongs into trusted Action results registered in mediator configuration via {nameof(IMediatorConfigurator.AddActionsFromAssembly)} or {nameof(IMediatorConfigurator.AddActionsFromAssemblyOf)} or .AddMediatorClient(o => o.CredibleResultTypes = new Type[]{{...}}).");
    }

    internal static MediatorHttpException CreateForNonContractType(Type queryType)
    {
        return new MediatorHttpException(
            $"Received contract type {queryType.FullName} does not implements interface {nameof(IMediatorAction)} and is not specified in CredibleResultAssemblies neither CredibleResultTypes.");
    }

    internal static MediatorHttpException CreateForNotConstructableJsonConverter()
    {
        return new MediatorHttpException("Can not activate JsonConverter class");
    }
}