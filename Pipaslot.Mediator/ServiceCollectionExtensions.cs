using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Services;

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
            services.AddScoped<IMediator, Mediator>();
            services.AddTransient<HandlerExistenceChecker>();
            var configurator = new PipelineConfigurator(services);
            services.AddSingleton(configurator);

            return configurator;
        }
    }
}
