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
        [Obsolete("Use AddMediatorServer() instead. Otherwise some important dependencies wont be registered")]
        public static IPipelineConfigurator AddMediator(this IServiceCollection services, Action<ServerMediatorOptions> configure)
        {
            var options = new ServerMediatorOptions();
            configure(options);
            services.AddSingleton(options);

            return services.AddMediator();
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
            //services.AddHttpContextAccessor();

            return services.AddMediator();
        }
    }
}
