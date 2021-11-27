using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Http.Middlewares;

namespace Pipaslot.Mediator.Http
{
    public static class ConditionalPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Register exception logging middleware writting all error into ILogger
        /// </summary>
        public static IConditionalPipelineConfigurator UseExceptionLogging(this IConditionalPipelineConfigurator configurator, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            configurator.Use<MediatorExceptionLoggingMiddleware>(lifetime);
            return configurator;
        }
    }
}
