using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;

namespace Pipaslot.Mediator.Configuration
{
    /// <summary>
    /// Register middlewares and their execution conditions
    /// </summary>
    public interface IMiddlewareRegistrator
    {
        /// <summary>
        /// Register middleware in pipeline for all actions
        /// </summary>
        /// <typeparam name="TMiddleware">Middleware type</typeparam>
        /// <param name="lifetime">Middleware lifetime set on service collection</param>
        IMiddlewareRegistrator Use<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware;

        /// <summary>
        /// Register middleware in pipeline for all actions. 
        /// </summary>
        /// <typeparam name="TMiddleware">Middleware type</typeparam>
        /// <param name="setupDependencies">Additional dependencies registered with middleware</param>
        /// <param name="lifetime">Middleware lifetime set on service collection</param>
        IMiddlewareRegistrator Use<TMiddleware>(Action<IServiceCollection> setupDependencies, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware;

        /// <summary>
        /// Register middlewares when the condition is met.
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="subMiddlewares"></param>
        /// <returns></returns>
        IMiddlewareRegistrator UseWhen(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares);
    }
}
