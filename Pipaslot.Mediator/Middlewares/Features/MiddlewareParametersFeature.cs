using Pipaslot.Mediator.Configuration;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Middlewares.Features
{
    /// <summary>
    /// Contains custom parameters provided during mediator pipeline configuration by <see cref="IMiddlewareRegistrator.Use{TMiddleware}(Microsoft.Extensions.DependencyInjection.ServiceLifetime, object?[])"/>. 
    /// </summary>
    public class MiddlewareParametersFeature
    {
        public static readonly MiddlewareParametersFeature Default = new MiddlewareParametersFeature(Array.Empty<object>());

        public MiddlewareParametersFeature(object[] parameters)
        {
            Parameters = parameters;
        }

        public IReadOnlyCollection<object> Parameters { get; }
    }
}
