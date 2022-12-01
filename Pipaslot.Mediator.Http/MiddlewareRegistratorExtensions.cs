using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Middlewares;

namespace Pipaslot.Mediator.Http
{
    public static class MiddlewareRegistratorExtensions
    {
        /// <summary>
        /// Register exception logging middleware writting all error into ILogger
        /// </summary>
        public static IMiddlewareRegistrator UseExceptionLogging(this IMiddlewareRegistrator configurator, ServiceLifetime lifetime = ServiceLifetime.Scoped)
        {
            configurator.Use<ExceptionLoggingMiddleware>(lifetime);
            return configurator;
        }

        /// <summary>
        /// Register middleware sending actions over HTTP client to mediator server implementation. Not further middleware will be executed after this one.
        /// </summary>
        public static IMiddlewareRegistrator UseHttpClient(this IMiddlewareRegistrator configurator)
        {
            configurator.Use<HttpClientExecutionMiddleware>();
            return configurator;
        }

        /// <summary>
        /// Register middleware sending actions over HTTP client to mediator server implementation. Not further middleware will be executed after this one.
        /// </summary>
        public static IMiddlewareRegistrator UseHttpClient<THttpClientExecutionMiddleware>(this IMiddlewareRegistrator configurator)
            where THttpClientExecutionMiddleware : HttpClientExecutionMiddleware
        {
            configurator.Use<THttpClientExecutionMiddleware>();
            return configurator;
        }

        /// <summary>
        /// Prevent direct calls for action which are not part of your application REST API. 
        /// Can be used as protection for queries placed in app demilitarized zone. Such a actions lacks authentication, authorization or different security checks.
        /// </summary>
        public static IMiddlewareRegistrator UseDirectHttpCallProtection(this IMiddlewareRegistrator config)
        {
            return config.Use<DirectHttpCallProtectionMiddleware>(ServiceLifetime.Scoped);
        }
    }
}
