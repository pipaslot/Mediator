using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using System;

namespace Pipaslot.Mediator
{
    public static class IPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Pipeline running all handlers concurrently. Not further pipeline will be executed after this one for specified Action Marker.
        /// </summary>
        /// <typeparam name="TActionMarker">Action interface</typeparam>
        [Obsolete("Pipeline definition was replaced by .AddPipeline<TActionMarker>(...).UseConcurrentMultiHandler()")]
        public static IPipelineConfigurator UseConcurrentMultiHandler<TActionMarker>(this IPipelineConfigurator config)
            where TActionMarker : IMediatorAction
        {
            return config.Use<MultiHandlerConcurrentExecutionMiddleware, TActionMarker>();
        }

        /// <summary>
        /// Pipeline running all handlers in sequence one by one. Not further pipeline will be executed after this one for specified Action Marker.
        /// For order specification see <see cref="ISequenceHandler"/>
        /// </summary>
        /// <typeparam name="TActionMarker">Action interface</typeparam>
        [Obsolete("Pipeline definition was replaced by .AddPipeline<TActionMarker>(...).UseSequenceMultiHandler()")]
        public static IPipelineConfigurator UseSequenceMultiHandler<TActionMarker>(this IPipelineConfigurator config)
            where TActionMarker : IMediatorAction
        {
            return config.Use<MultiHandlerSequenceExecutionMiddleware, TActionMarker>();
        }
    }
}
