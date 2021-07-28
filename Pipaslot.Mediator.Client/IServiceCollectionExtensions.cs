using Microsoft.Extensions.DependencyInjection;
using System;

namespace Pipaslot.Mediator.Client
{
    public static class IServiceCollectionExtensions
    {
        /// <summary>
        /// Register Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        public static IServiceCollection AddMediatorClient(this IServiceCollection services)
        {
            return services.AddMediatorClient<ClientMediator>(o => { });
        }
        /// <summary>
        /// Register Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configure">Mediator configuration</param>
        public static IServiceCollection AddMediatorClient(this IServiceCollection services, Action<ClientMediatorOptions> configure)
        {
            return services.AddMediatorClient<ClientMediator>(configure);
        }

        /// <summary>
        /// Register Custom Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        public static IServiceCollection AddMediatorClient<TClientMediator>(this IServiceCollection services) where TClientMediator : ClientMediator
        {
            return services.AddMediatorClient<TClientMediator>(o => { });
        }

        /// <summary>
        /// Register Custom Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        /// <param name="configure">Mediator configuration</param>
        public static IServiceCollection AddMediatorClient<TClientMediator>(this IServiceCollection services, Action<ClientMediatorOptions> configure) where TClientMediator : ClientMediator
        {
            var options = new ClientMediatorOptions();
            configure(options);
            services.AddSingleton(options);
            services.AddScoped<IMediator, TClientMediator>();
            return services;
        }
    }
}
