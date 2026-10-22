using Pipaslot.Mediator.Configuration;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Middlewares.Features;

/// <summary>
/// Contains custom parameters provided during mediator pipeline configuration by <see cref="IMiddlewareRegistrator.Use{TMiddleware}(Microsoft.Extensions.DependencyInjection.ServiceLifetime,object[])"/>. 
/// </summary>
public class MiddlewareParametersFeature(object[] parameters)
{
    /// <summary>
    /// Instance without parameters, present for a middleware registered without any. Reading the feature therefore never
    /// returns null and never returns parameters of a different middleware in the same pipeline.
    /// </summary>
    public static readonly MiddlewareParametersFeature Default = new([]);

    /// <summary>
    /// Parameters passed to the registration of the middleware currently being invoked, in the order they were passed.
    /// The feature is replaced before each middleware runs, so read it in <see cref="IMediatorMiddleware.Invoke"/> before calling <c>next</c>.
    /// </summary>
    public IReadOnlyCollection<object> Parameters { get; } = parameters;
}