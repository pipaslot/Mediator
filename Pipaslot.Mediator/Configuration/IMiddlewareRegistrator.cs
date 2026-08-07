using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using System;

namespace Pipaslot.Mediator.Configuration;

/// <summary>
/// Register middlewares and their execution conditions
/// </summary>
/// <remarks>
/// Registration order is execution order. The same registrator shape is used for the default pipeline (through
/// <see cref="IMediatorConfigurator"/>) and for a named one built by <see cref="IMediatorConfigurator.AddPipeline"/>, so
/// the choice is between conditionally adding middlewares to one pipeline (<see cref="UseWhen(Func{IMediatorAction,bool},Action{IMiddlewareRegistrator})"/>)
/// and replacing the pipeline entirely for a set of actions (<see cref="IMediatorConfigurator.AddPipeline"/>).
/// </remarks>
public interface IMiddlewareRegistrator
{
    /// <summary>
    /// Register middleware in pipeline for all actions
    /// </summary>
    /// <remarks>
    /// The middleware type is registered in the service collection as a side effect, so it does not have to be added
    /// there separately - which also means <paramref name="lifetime"/> is what decides how often it is constructed.
    /// Scoped by default; use <see cref="Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton"/> for a
    /// stateless middleware on a hot path, and note that a singleton middleware must not capture scoped services in
    /// fields. Registering the same middleware type twice with different lifetimes throws <see cref="MediatorException"/>.
    /// </remarks>
    /// <typeparam name="TMiddleware">Middleware type</typeparam>
    /// <param name="lifetime">Middleware lifetime set on service collection</param>
    /// <param name="parameters">Parameters passed to <see cref="MediatorContext.Features"/> right before middleware execution and available under type <see cref="MiddlewareParametersFeature"/></param>
    IMiddlewareRegistrator Use<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Scoped, object[]? parameters = null)
        where TMiddleware : IMediatorMiddleware;

    /// <summary>
    /// Register middleware in pipeline for all actions.
    /// </summary>
    /// <remarks>
    /// Same as the overload without <paramref name="setupDependencies"/>, plus a hook for registering what the middleware
    /// injects - use it to keep a middleware's own dependencies next to its registration instead of scattering them over
    /// the application's DI setup.
    /// </remarks>
    /// <typeparam name="TMiddleware">Middleware type</typeparam>
    /// <param name="setupDependencies">Additional dependencies registered with middleware</param>
    /// <param name="lifetime">Middleware lifetime set on service collection</param>
    /// <param name="parameters">Parameters passed to <see cref="MediatorContext.Features"/> right before middleware execution and available under type <see cref="MiddlewareParametersFeature"/></param>
    IMiddlewareRegistrator Use<TMiddleware>(Action<IServiceCollection> setupDependencies, ServiceLifetime lifetime = ServiceLifetime.Scoped,
        object[]? parameters = null) where TMiddleware : IMediatorMiddleware;

    /// <summary>
    /// Register middlewares when the condition is met.
    /// </summary>
    /// <remarks>
    /// A branch inside one pipeline, not a pipeline of its own: the surrounding middlewares still run, and the nested ones
    /// keep their position in the registration order. The condition is evaluated per action on every execution, so keep it
    /// cheap and side-effect free - typically a type check such as <c>action is IMyMarker</c>. Use
    /// <see cref="IMediatorConfigurator.AddPipeline"/> instead when a set of actions needs a different pipeline rather
    /// than an extra step in the shared one.
    /// </remarks>
    /// <param name="condition">Evaluated for every dispatched action</param>
    /// <param name="subMiddlewares">Middlewares applied when condition is met</param>
    IMiddlewareRegistrator UseWhen(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares);

    /// <summary>
    /// Register middlewares when the condition is met.
    /// </summary>
    /// <remarks>
    /// Same branching as the overload taking only the action, for a condition that has to resolve a service - a feature
    /// flag or a per-tenant setting. Resolve as little as possible: this runs before every matching action.
    /// </remarks>
    /// <param name="condition">Evaluated for every dispatched action, with the service provider of the running execution</param>
    /// <param name="subMiddlewares">Middlewares applied when condition is met</param>
    IMiddlewareRegistrator UseWhen(Func<IMediatorAction, IServiceProvider, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares);
}