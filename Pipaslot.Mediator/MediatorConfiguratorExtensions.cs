using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares.Pipelines;
using System;
using System.Linq;

namespace Pipaslot.Mediator;

/// <summary>
/// Shorthands for <see cref="IMediatorConfigurator.AddPipeline"/> with a predicate or with marker types as the condition.
/// </summary>
/// <remarks>
/// Every overload registers a pipeline that replaces the default one for the matching actions, under the same rules as
/// <see cref="IMediatorConfigurator.AddPipeline"/> - notably that an action matching more than one pipeline fails on
/// execution with <see cref="MediatorException.TooManyPipelines"/>. To add a middleware for a subset of actions while
/// keeping the shared pipeline, use the <c>UseWhen*</c> methods of <see cref="MiddlewareRegistratorExtensions"/> instead.
/// </remarks>
public static class MediatorConfiguratorExtensions
{
    /// <summary>
    /// Register middlewares as pipeline executed independently of the default pipeline
    /// </summary>
    /// <param name="configurator">The configurator returned by <c>AddMediator</c></param>
    /// <param name="condition">Evaluated for every dispatched action; true selects this pipeline instead of the default one</param>
    /// <param name="subMiddlewares">Middlewares applied when condition is met</param>
    /// <param name="identifier">Customized unique pipeline identifier. Pipeline with the same identifier will be replaced</param>
    public static IMediatorConfigurator AddPipeline(this IMediatorConfigurator configurator, Func<IMediatorAction, bool> condition,
        Action<IMiddlewareRegistrator> subMiddlewares,
        string? identifier = null)
    {
        configurator.AddPipeline(new FuncPipelineCondition(condition), subMiddlewares, identifier);
        return configurator;
    }
    
    /// <summary>
    /// Register action-specific pipeline with separated middlewares applied only for specified action types.
    /// </summary>
    /// <remarks>
    /// Matches actions assignable to any of <paramref name="actionTypes"/>, so a marker interface covers every action
    /// implementing it. The pipeline identifier is derived from the types, so calling this again with the same types in the
    /// same order replaces the earlier pipeline instead of adding a competing one.
    /// </remarks>
    public static IMediatorConfigurator AddPipelineForActions(this IMediatorConfigurator configurator, Action<IMiddlewareRegistrator> subMiddlewares,
        params Type[] actionTypes)
    {
        var name = string.Join("-", actionTypes.Select(t => t.ToString()));
        return configurator.AddPipeline(new TypeBasedPipelineCondition(actionTypes), 
            subMiddlewares,
            name);
    }
    
    /// <summary>
    /// Register action-specific pipeline with separated middlewares applied only for actions implementing TActionMarker.
    /// </summary>
    public static IMediatorConfigurator AddPipelineForAction<TActionMarker>(this IMediatorConfigurator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.AddPipelineForActions(subMiddlewares, typeof(TActionMarker));
    }

    /// <summary>
    /// Register action-specific pipeline with separated middlewares applied only for actions implementing any TActionMarker.
    /// </summary>
    public static IMediatorConfigurator AddPipelineForActions<TActionMarker1, TActionMarker2>(this IMediatorConfigurator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.AddPipelineForActions(subMiddlewares, typeof(TActionMarker1), typeof(TActionMarker2));
    }

    /// <summary>
    /// Register action-specific pipeline with separated middlewares applied only for actions implementing any TActionMarker.
    /// </summary>
    public static IMediatorConfigurator AddPipelineForActions<TActionMarker1, TActionMarker2, TActionMarker3>(this IMediatorConfigurator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.AddPipelineForActions(subMiddlewares, typeof(TActionMarker1), typeof(TActionMarker2), typeof(TActionMarker3));
    }

    /// <summary>
    /// Create or override middlewares used for Authorization requests
    /// </summary>
    /// <remarks>
    /// <c>AddMediator</c> already registers an empty pipeline for <see cref="AuthorizeRequest"/>, the action behind
    /// <see cref="MediatorExtensions.IsAuthorized"/> and <see cref="MediatorExtensions.Authorize"/>, so that custom
    /// middlewares of the default pipeline do not interfere with authorization checks; this call replaces it.
    /// </remarks>
    public static IMediatorConfigurator AddPipelineForAuthorizationRequest(this IMediatorConfigurator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.AddPipelineForAction<AuthorizeRequest>(subMiddlewares);
    }
}