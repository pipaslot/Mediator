using Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped<IMediator, ClientMediator>();
            return services;
        }

        /// <summary>
        /// Register Custom Mediator sending messages and requests over HTTPClient
        /// </summary>
        /// <param name="services">Service collection</param>
        public static IServiceCollection AddMediatorClient<TClientMediator>(this IServiceCollection services) where TClientMediator : ClientMediator
        {
            services.AddScoped<IMediator, TClientMediator>();
            return services;
        }
    }
}
