using System;

namespace Pipaslot.Mediator.Middlewares;

/// <summary>
/// Stands in for the service provider of a context created outside a mediator execution and without services.
/// Fails fast with an actionable message instead of letting the missing provider surface as a
/// <see cref="NullReferenceException"/> from somewhere deeper in the pipeline.
/// </summary>
internal sealed class DetachedServiceProvider : IServiceProvider
{
    internal static readonly DetachedServiceProvider Instance = new();

    private DetachedServiceProvider()
    {
    }

    public object? GetService(Type serviceType)
    {
        throw MediatorException.CreateForContextWithoutServices(serviceType);
    }
}
