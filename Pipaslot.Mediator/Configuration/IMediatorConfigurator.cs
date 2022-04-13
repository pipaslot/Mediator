﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
using System.Reflection;

namespace Pipaslot.Mediator.Configuration
{
    /// <summary>
    /// Commont configuration for all pipelines and for handler processing. Scans assemblies for action markers and their handlers. Pipeline is specified by registered middlewares by their order
    /// </summary>
    public interface IMediatorConfigurator : IMiddlewareRegistrator
    {
        /// <summary>
        /// Scan assemblies for action handler types
        /// </summary>
        IMediatorConfigurator AddHandlersFromAssembly(params Assembly[] assemblies);

        /// <summary>
        /// Will scan for action handlers from the assembly of type <typeparamref name="T"/> and register them.
        /// </summary>
        /// <typeparam name="T">The type from target asssembly to be scanned</typeparam>
        IMediatorConfigurator AddHandlersFromAssemblyOf<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient);

        /// <summary>
        /// Will scan for action markers from the passed assemblies and register them.
        /// </summary>
        IMediatorConfigurator AddActionsFromAssembly(params Assembly[] assemblies);

        /// <summary>
        /// Will scan for action markers from the assembly of type <typeparamref name="T"/> and register them.
        /// </summary>
        /// <typeparam name="T">The type from target asssembly to be scanned</typeparam>
        IMediatorConfigurator AddActionsFromAssemblyOf<T>();

        /// <summary>
        /// Register pipeline which middlewares will be executed independently
        /// </summary>
        /// <param name="condition"></param>
        /// <param name="subMiddlewares">Middlewares applied when condition is met</param>
        IMediatorConfigurator AddPipeline(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares);
    }
}