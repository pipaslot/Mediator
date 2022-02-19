using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
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
            configurator.Use<ExceptionLoggingMiddleware>(lifetime);
            return configurator;
        }

        /// <summary>
        /// Register middleware sending actions over HTTP client to mediator server implementation. Not further middleware will be executed after this one.
        /// </summary>
        public static IConditionalPipelineConfigurator UseHttpClient(this IConditionalPipelineConfigurator configurator)
        {
            configurator.Use<HttpClientExecutionMiddleware>();
            return configurator;
        }

        /// <summary>
        /// Register middleware sending actions over HTTP client to mediator server implementation. Not further middleware will be executed after this one.
        /// </summary>
        public static IConditionalPipelineConfigurator UseHttpClient<THttpClientExecutionMiddleware>(this IConditionalPipelineConfigurator configurator)
            where THttpClientExecutionMiddleware : HttpClientExecutionMiddleware
        {
            configurator.Use<THttpClientExecutionMiddleware>();
            return configurator;
        }
    }
}
