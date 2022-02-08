using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using System;

namespace Pipaslot.Mediator.Configuration
{
    /// <summary>
    /// Define single pipeline definition. Mulptiple pilepinec can be registered but only single one will be always applied.
    /// </summary>
    public interface IConditionalPipelineConfigurator : IPipelineRegistrator
    {
        /// <summary>
        /// Register middleware in pipeline for all actions
        /// </summary>
        /// <typeparam name="TMiddleware">Middleware type</typeparam>
        /// <param name="lifetime">Middleware lifetime set on service collection</param>
        IConditionalPipelineConfigurator Use<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware;

        /// <summary>
        /// Register middleware in pipeline for all actions. 
        /// </summary>
        /// <typeparam name="TMiddleware">Middleware type</typeparam>
        /// <param name="setupDependencies">Additional dependencies registered with middleware</param>
        /// <param name="lifetime">Middleware lifetime set on service collection</param>
        IConditionalPipelineConfigurator Use<TMiddleware>(Action<IServiceCollection> setupDependencies, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware;
    }
}
