using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Reflection;

namespace Pipaslot.Mediator.Configuration
{
    /// <summary>
    /// Commont configuration for all pipelines and for handler processing. Scans assemblies for action markers and their handlers. Pipeline is specified by registered middlewares by their order
    /// </summary>
    public interface IPipelineConfigurator : IPipelineRegistrator
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
        IPipelineConfigurator AddActionsFromAssembly(params Assembly[] assemblies);

        /// <summary>
        /// Will scan for action markers from the assembly of type <typeparamref name="T"/> and register them.
        /// </summary>
        /// <typeparam name="T">The type from target asssembly to be scanned</typeparam>
        IPipelineConfigurator AddActionsFromAssemblyOf<T>();        
    }
}