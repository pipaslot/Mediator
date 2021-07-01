using Pipaslot.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Configures handler sources and pipeline for handler processing.
        /// Every Request/Message is configured to have exactly one handler by default.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IPipelineConfigurator AddMediator(this IServiceCollection services)
        {
            services.AddTransient<ServiceResolver>();
            services.AddTransient<IMediator, Mediator>();
            services.AddTransient<HandlerExistenceChecker>();
            var configurator = new PipelineConfigurator(services);
            services.AddSingleton(configurator);

            return configurator;
        }
    }
}
