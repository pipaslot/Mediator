using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using System;

namespace Pipaslot.Mediator.Server
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures handler sources and pipeline for handler processing.
        /// Every Request/Message is configured to have exactly one handler by default.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configure">Mediator configuration</param>
        /// <returns></returns>
        public static IPipelineConfigurator AddMediator(this IServiceCollection services, Action<ServerMediatorOptions> configure)
        {
            var options = new ServerMediatorOptions();
            configure(options);
            services.AddSingleton(options);

            return services.AddMediator();
        }
    }
}
