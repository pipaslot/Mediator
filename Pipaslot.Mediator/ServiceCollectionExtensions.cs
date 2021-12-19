using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
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
        public static IPipelineConfigurator AddMediator(this IServiceCollection services)
        {
            return services.AddMediator<SingleHandlerExecutionMiddleware>();
        }

        /// <summary>
        /// Configures handler sources and pipeline for handler processing.
        /// Every Request/Message is configured to have exactly one handler by default.
        /// </summary>
        /// <typeparam name="TDefaultExecutionMiddleware">Default handler executive middleware ised in case when no other middleware is registered</typeparam>
        /// <param name="services"></param>
        public static IPipelineConfigurator AddMediator<TDefaultExecutionMiddleware>(this IServiceCollection services) where TDefaultExecutionMiddleware : class, IExecutionMiddleware
        {
            services.AddScoped<IMediator, Mediator>();
            services.AddTransient<IHandlerExistenceChecker, HandlerExistenceChecker>();
            var configurator = new PipelineConfigurator(services);
            services.AddSingleton(configurator);
            services.AddScoped<IExecutionMiddleware, TDefaultExecutionMiddleware>();

            return configurator;
        }
    }
}
