using Pipaslot.Mediator.Configuration;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Middlewares.Features;

/// <summary>
/// Contains custom parameters provided during mediator pipeline configuration by <see cref="IMiddlewareRegistrator.Use{TMiddleware}(Microsoft.Extensions.DependencyInjection.ServiceLifetime,object[])"/>. 
/// </summary>
public class MiddlewareParametersFeature(object[] parameters)
{
    public static readonly MiddlewareParametersFeature Default = new([]);

    public IReadOnlyCollection<object> Parameters { get; } = parameters;
}