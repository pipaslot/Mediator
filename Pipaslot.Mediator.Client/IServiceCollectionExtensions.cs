using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http;
using System;

namespace Pipaslot.Mediator.Client
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Register Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        public static IPipelineConfigurator AddMediatorClient(this IServiceCollection services)
        {
            return services.AddMediatorClient<HttpClientExecutionMiddleware>(o => { });
        }
        /// <summary>
        /// Register Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configure">Mediator configuration</param>
        public static IPipelineConfigurator AddMediatorClient(this IServiceCollection services, Action<ClientMediatorOptions> configure)
        {
            return services.AddMediatorClient<HttpClientExecutionMiddleware>(configure);
        }

        /// <summary>
        /// Register Custom Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        public static IPipelineConfigurator AddMediatorClient<THttpClientExecutionMiddleware>(this IServiceCollection services) where THttpClientExecutionMiddleware : HttpClientExecutionMiddleware
        {
            return services.AddMediatorClient<THttpClientExecutionMiddleware>(o => { });
        }

        /// <summary>
        /// Register Custom Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configure">Mediator configuration</param>
        public static IPipelineConfigurator AddMediatorClient<THttpClientExecutionMiddleware>(this IServiceCollection services, Action<ClientMediatorOptions> configure) where THttpClientExecutionMiddleware : HttpClientExecutionMiddleware
        {
            var options = new ClientMediatorOptions();
            configure(options);
            services.AddSingleton(options);
            services.AddSingleton<IContractSerializer, ContractSerializer>();
            return services.AddMediator<THttpClientExecutionMiddleware>();
        }
    }
}
