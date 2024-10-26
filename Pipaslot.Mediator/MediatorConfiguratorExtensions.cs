﻿using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using System;
using System.Linq;

namespace Pipaslot.Mediator;

public static class MediatorConfiguratorExtensions
{
    /// <summary>
    /// Register action-specific pipeline with separated middlewares applied only for specified action types.
    /// </summary>
    public static IMediatorConfigurator AddPipelineForActions(this IMediatorConfigurator configurator, Action<IMiddlewareRegistrator> subMiddlewares,
        params Type[] actionTypes)
    {
        var name = string.Join("-", actionTypes.Select(t => t.ToString()));
        return configurator.AddPipeline(action =>
            {
                var actionType = action.GetType();
                return actionTypes.Any(mt => mt.IsAssignableFrom(actionType));
            },
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
    public static IMediatorConfigurator AddPipelineForAuthorizationRequest(this IMediatorConfigurator configurator,
        Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return configurator.AddPipelineForAction<AuthorizeRequest>(subMiddlewares);
    }
}