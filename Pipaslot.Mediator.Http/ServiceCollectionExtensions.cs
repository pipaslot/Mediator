using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Middlewares;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.Linq;

namespace Pipaslot.Mediator.Http
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Register Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        public static IMediatorConfigurator AddMediatorClient(this IServiceCollection services)
        {
            return services.AddMediatorClient<HttpClientExecutionMiddleware>(o => { });
        }
        /// <summary>
        /// Register Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configure">Mediator configuration</param>
        public static IMediatorConfigurator AddMediatorClient(this IServiceCollection services, Action<ClientMediatorOptions> configure)
        {
            return services.AddMediatorClient<HttpClientExecutionMiddleware>(configure);
        }

        /// <summary>
        /// Register Custom Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        public static IMediatorConfigurator AddMediatorClient<THttpClientExecutionMiddleware>(this IServiceCollection services) where THttpClientExecutionMiddleware : HttpClientExecutionMiddleware
        {
            return services.AddMediatorClient<THttpClientExecutionMiddleware>(o => { });
        }

        /// <summary>
        /// Register Custom Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configure">Mediator configuration</param>
        public static IMediatorConfigurator AddMediatorClient<THttpClientExecutionMiddleware>(this IServiceCollection services, Action<ClientMediatorOptions> configure) where THttpClientExecutionMiddleware : HttpClientExecutionMiddleware
        {
            var options = new ClientMediatorOptions();
            configure(options);
            services.AddSingleton(options);
            services.AddSingleton((IMediatorOptions)options);
            if (options.DeserializeOnlyCredibleResultTypes)
            {
                services.AddSingleton<ICredibleProvider>(s =>
                {
                    var pipConf = s.GetRequiredService<MediatorConfigurator>();
                    return new CredibleResultProvider(pipConf, options.CredibleResultTypes, options.CredibleResultAssemblies);
                });
            }
            else
            {
                services.AddSingleton<ICredibleProvider, NopCredibleProvider>();
            }
            if (options.SerializerType == SerializerType.V3)
            {
                services.AddSingleton<IContractSerializer, Serialization.V3.JsonContractSerializer>();
            }
            else
            {
                services.AddSingleton<IContractSerializer, Serialization.V2.FullJsonContractSerializer>();
            }
            services.AddScoped<IMediatorUrlFormatter, THttpClientExecutionMiddleware>();
            return services.AddMediator<THttpClientExecutionMiddleware>();
        }

        /// <summary>
        /// Configures handler sources and pipeline for handler processing.
        /// Every Request/Message is configured to have exactly one handler by default.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IMediatorConfigurator AddMediatorServer(this IServiceCollection services)
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
        public static IMediatorConfigurator AddMediatorServer(this IServiceCollection services, Action<ServerMediatorOptions> configure)
        {
            var options = new ServerMediatorOptions();
            configure(options);
            services.AddSingleton(options);
            services.AddSingleton((IMediatorOptions)options);
            if (options.DeserializeOnlyCredibleActionTypes)
            {
                services.AddSingleton<ICredibleProvider>(s =>
                {
                    var pipConf = s.GetRequiredService<MediatorConfigurator>();
                    return new CredibleActionProvider(pipConf, options.CredibleResultTypes, options.CredibleResultAssemblies);

                });
            }
            else
            {
                services.AddSingleton<ICredibleProvider, NopCredibleProvider>();
            }
            if (options.SerializerType == SerializerType.V3)
            {
                services.AddSingleton<IContractSerializer, Serialization.V3.JsonContractSerializer>();
            }
            else
            {
                services.AddSingleton<IContractSerializer, Serialization.V2.FullJsonContractSerializer>();
            }

            var config = services.AddMediator();

            var existingClaimPrincipalAccessors = services.Where(s => s.ImplementationType == typeof(IClaimPrincipalAccessor));
            foreach (var existingClaimPrincipalAccessor in existingClaimPrincipalAccessors)
            {
                services.Remove(existingClaimPrincipalAccessor);
            }
            services.AddSingleton<IClaimPrincipalAccessor, ClaimPrincipalAccessor>();
            return config;
        }
    }
}
