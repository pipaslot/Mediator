using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Middlewares;
using Pipaslot.Mediator.Http.Configuration;
using System;
using System.Linq;
using Pipaslot.Mediator.Http.Serialization;

namespace Pipaslot.Mediator.Http
{
    public static class ServiceCollectionExtensions
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
            services.AddSingleton<ICredibleActionProvider, NopCredibleActionProvider>();
            if (options.DeserializeOnlyCredibleResultTypes)
            {
                services.AddSingleton<ICredibleResultProvider, CredibleResultProvider>();
            }
            else
            {
                services.AddSingleton<ICredibleResultProvider, NopCredibleResultProvider>();
            }
            services.AddSingleton<IContractSerializer, FullJsonContractSerializer>();
            return services.AddMediator<THttpClientExecutionMiddleware>();
        }

        /// <summary>
        /// Configures handler sources and pipeline for handler processing.
        /// Every Request/Message is configured to have exactly one handler by default.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IPipelineConfigurator AddMediatorServer(this IServiceCollection services)
        {
            return services.AddMediatorServer(o => { });
        }

        /// <summary>
        /// Configures handler sources and pipeline for handler processing.
        /// Every Request/Message is configured to have exactly one handler by default.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure">Mediator configuration</param>
        /// <returns></returns>
        public static IPipelineConfigurator AddMediatorServer(this IServiceCollection services, Action<ServerMediatorOptions> configure)
        {
            var options = new ServerMediatorOptions();
            configure(options);
            services.AddSingleton(options);
            if (options.DeserializeOnlyCredibleActionTypes)
            {
                services.AddSingleton<ICredibleActionProvider, CredibleActionProvider>();
            }
            else
            {
                services.AddSingleton<ICredibleActionProvider, NopCredibleActionProvider>();
            }
            services.AddSingleton<ICredibleResultProvider, NopCredibleResultProvider>();
            services.AddSingleton<IContractSerializer, FullJsonContractSerializer>();

            return services.AddMediator();
        }

        /// <summary>
        /// Replace mediator serializer used by Pipaslot.Mediator.Client and Pipaslot.Mediator.Server in versions 2.0 and 3.0
        /// Register this service after AddMediatorClient() and AddMediatorServer()
        /// </summary>
        public static IServiceCollection UseMediatorSerializerV2(this IServiceCollection services)
        {
            var descriptor = services.First(d => d.ServiceType == typeof(IContractSerializer));
            services.Remove(descriptor);
            services.AddSingleton<IContractSerializer, ContractSerializer>();
            return services;
        }
    }
}
