using System;

namespace Pipaslot.Mediator.Configuration
{
    public static class IMediatorConfiguratorExtensions
    {
        /// <summary>
        /// Register action-specific pipeline with separated middlewares applied only for actions implementing TActionMarker.
        /// </summary>
        [Obsolete("Use Pipaslot.Mediator.MediatorConfiguratorExtensions.AddPipelineForAction istead")]
        public static IMediatorConfigurator AddPipelineForAction<TActionMarker>(this IMediatorConfigurator configurator, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            return configurator.AddPipeline(action => typeof(TActionMarker).IsAssignableFrom(action.GetType()), subMiddlewares, nameof(TActionMarker));
        }
    }
}