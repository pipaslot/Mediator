using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using System;

namespace Pipaslot.Mediator
{
    public static class MediatorConfiguratorExtensions
    {
        /// <summary>
        /// Register action-specific pipeline with separated middlewares applied only for actions implementing TActionMarker.
        /// </summary>
        public static IMediatorConfigurator AddPipelineForAction<TActionMarker>(this IMediatorConfigurator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.AddPipeline(action => typeof(TActionMarker).IsAssignableFrom(action.GetType()), subMiddlewares, nameof(TActionMarker));
        }

        /// <summary>
        /// Create or override middlewares used for Authorization requests
        /// </summary>
        public static IMediatorConfigurator AddPipelineForAuthorizationRequest(this IMediatorConfigurator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.AddPipelineForAction<IsAuthorizedRequest>(subMiddlewares);
        }
    }
}
