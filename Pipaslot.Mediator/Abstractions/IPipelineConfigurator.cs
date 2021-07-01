using System.Reflection;

namespace Pipaslot.Mediator.Abstractions
{
    /// <summary>
    /// Configure pipeline for handler processing. Scans assemblies for action markers and their handlers. Pipeline is specified by registered middlewares by their order
    /// </summary>
    public interface IPipelineConfigurator
    {
        /// <summary>
        /// Scan assemblies for action handler types
        /// </summary>
        IPipelineConfigurator AddHandlersFromAssembly(params Assembly[] assemblies);

        /// <summary>
        /// Will scan for action handlers from the assembly of type <typeparamref name="T"/> and register them.
        /// </summary>
        /// <typeparam name="T">The type from target asssembly to be scanned</typeparam>
        IPipelineConfigurator AddHandlersFromAssemblyOf<T>();

        /// <summary>
        /// Will scan for action markers from the passed assemblies and register them.
        /// </summary>
        IPipelineConfigurator AddMarkersFromAssemblyOf(params Assembly[] assemblies);

        /// <summary>
        /// Will scan for action markers from the assembly of type <typeparamref name="T"/> and register them.
        /// </summary>
        /// <typeparam name="T">The type from target asssembly to be scanned</typeparam>
        IPipelineConfigurator AddMarkersFromAssemblyOf<T>();

        /// <summary>
        /// Register middleware in pipeline for all actions
        /// </summary>
        IPipelineConfigurator Use<TMiddleware>() where TMiddleware : IMediatorMiddleware;

        /// <summary>
        /// Register middleware in pipeline for action classes implementing marker type only
        /// </summary>
        /// <typeparam name="TMiddleware">Middleware to apply</typeparam>
        /// <typeparam name="TActionMarker">Action interface</typeparam>
        IPipelineConfigurator Use<TMiddleware, TActionMarker>()
            where TMiddleware : IMediatorMiddleware
            where TActionMarker : IActionMarker;
    }
}