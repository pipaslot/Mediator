using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Middlewares;
using Pipaslot.Mediator.Http.Serialization;
using System.Net.Http;

namespace Pipaslot.Mediator.Http.Tests;

/// <summary>
/// Covers the <see cref="MiddlewareRegistratorExtensions"/> shortcuts whose behavior does not depend on any runtime
/// condition - they unconditionally forward to <see cref="IMiddlewareRegistrator.Use{TMiddleware}"/>, so a mocked
/// <see cref="IMiddlewareRegistrator"/> is enough to prove the forwarding is correct; no DI container or dispatch is
/// needed. <see cref="MiddlewareRegistratorExtensions.UseExceptionLogging"/> is already covered end to end by
/// <c>E2E.ExceptionLoggingMiddlewareTests</c> and <see cref="MiddlewareRegistratorExtensions.UseDirectHttpCallProtection"/>
/// by <c>Middlewares.DirectHttpCallProtectionMiddlewareTests</c>, so neither is duplicated here.
/// The direct-vs-nested HTTP call gating extensions (<c>UseWhenDirectHttpCall</c>, <c>UseWhenNotDirectHttpCall</c>,
/// <c>UseAuthorizationWhenDirectHttpCall</c>) do depend on a runtime condition actually being evaluated against a
/// real call tree, so they are covered separately in <c>E2E.DirectHttpCallGatingExtensionsTests</c> instead of here.
/// </summary>
public class MiddlewareRegistratorExtensionsTests
{
    [Fact]
    public void UseHttpClient_RegistersHttpClientExecutionMiddleware()
    {
        var registrator = Substitute.For<IMiddlewareRegistrator>();
        registrator.Use<HttpClientExecutionMiddleware>(ServiceLifetime.Scoped, null).Returns(registrator);

        registrator.UseHttpClient();

        registrator.Received(1).Use<HttpClientExecutionMiddleware>(ServiceLifetime.Scoped, null);
    }

    [Fact]
    public void UseHttpClientGeneric_RegistersProvidedMiddlewareType_NotBaseType()
    {
        var registrator = Substitute.For<IMiddlewareRegistrator>();
        registrator.Use<CustomHttpClientExecutionMiddleware>(ServiceLifetime.Scoped, null).Returns(registrator);

        registrator.UseHttpClient<CustomHttpClientExecutionMiddleware>();

        registrator.Received(1).Use<CustomHttpClientExecutionMiddleware>(ServiceLifetime.Scoped, null);
    }

    private class CustomHttpClientExecutionMiddleware(
        HttpClient httpClient,
        ClientMediatorOptions options,
        IContractSerializer serializer,
        ILogger<HttpClientExecutionMiddleware> logger)
        : HttpClientExecutionMiddleware(httpClient, options, serializer, logger);
}
