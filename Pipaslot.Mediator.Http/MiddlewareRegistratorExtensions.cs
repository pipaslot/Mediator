using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Middlewares;
using Pipaslot.Mediator.Middlewares;
using System;

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

        #region UseWhenDirectHttpCall

        /// <summary>
        /// Use middleware when the provider action is first in line directly from HTTP.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenDirectHttpCall<TMiddleware>(this IMiddlewareRegistrator config)
            where TMiddleware : IMediatorMiddleware
        {
            return config.UseWhenDirectHttpCall(m => m.Use<TMiddleware>());
        }

        /// <summary>
        /// Use middlewares when the provider action is first in line directly from HTTP.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenDirectHttpCall(this IMiddlewareRegistrator config, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return config.UseWhen((a, s) => IsFromHttp(s), subMiddlewares);
        }

        #endregion

        #region UseWhenNotDirectHttpCall

        /// <summary>
        /// Use middleware when the provider action is first in line directly from HTTP.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenNotDirectHttpCall<TMiddleware>(this IMiddlewareRegistrator config)
            where TMiddleware : IMediatorMiddleware
        {
            return config.UseWhenNotDirectHttpCall(m => m.Use<TMiddleware>());
        }

        /// <summary>
        /// Use middlewares when the provider action is first in line directly from HTTP.
        /// </summary>
        public static IMiddlewareRegistrator UseWhenNotDirectHttpCall(this IMiddlewareRegistrator config, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return config.UseWhen((a, s) => IsFromHttp(s) == false, subMiddlewares);
        }

        #endregion

        /// <summary>
        /// Use middlewares when the provider action is first in line directly from HTTP.
        /// </summary>
        public static IMiddlewareRegistrator UseAuthorizationWhenDirectHttpCall(this IMiddlewareRegistrator config)
        {
            return config.UseWhen((a, s) => IsFromHttp(s), m => m.Use<AuthorizationMiddleware>(ServiceLifetime.Singleton));
        }

        private static bool IsFromHttp(IServiceProvider sp)
        {
            var hca = sp.GetRequiredService<IHttpContextAccessor>();
            var mca = sp.GetRequiredService<IMediatorContextAccessor>();
            return DirectHttpCallProtectionMiddleware.IsApplicable(mca, hca);
        }
    }
}
