using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Internal;
using Pipaslot.Mediator.Http.Middlewares;
using Pipaslot.Mediator.Middlewares;
using System;

namespace Pipaslot.Mediator.Http;

public static class MiddlewareRegistratorExtensions
{
    /// <inheritdoc cref="ExceptionLoggingMiddleware"/>
    public static IMiddlewareRegistrator UseExceptionLogging(this IMiddlewareRegistrator configurator,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        configurator.Use<ExceptionLoggingMiddleware>(lifetime);
        return configurator;
    }

    /// <inheritdoc cref="HttpClientExecutionMiddleware"/>
    public static IMiddlewareRegistrator UseHttpClient(this IMiddlewareRegistrator configurator)
    {
        configurator.Use<HttpClientExecutionMiddleware>();
        return configurator;
    }

    /// <inheritdoc cref="UseHttpClient"/>
    public static IMiddlewareRegistrator UseHttpClient<THttpClientExecutionMiddleware>(this IMiddlewareRegistrator configurator)
        where THttpClientExecutionMiddleware : HttpClientExecutionMiddleware
    {
        configurator.Use<THttpClientExecutionMiddleware>();
        return configurator;
    }

}