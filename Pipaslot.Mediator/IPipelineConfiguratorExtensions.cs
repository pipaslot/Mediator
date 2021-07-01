using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator
{
    public static class IPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Pipeline running all handlers concurrently. Not further pipeline will be executed after this one for specified Action Marker.
        /// </summary>
        /// <typeparam name="TActionMarker">Action interface</typeparam>
        public static IPipelineConfigurator UseConcurrentMultiHandler<TActionMarker>(this IPipelineConfigurator config)
            where TActionMarker : IActionMarker
        {
            return config.Use<MultiHandlerConcurrentExecutionMiddleware, TActionMarker>();
        }

        /// <summary>
        /// Pipeline running all handlers in sequence one by one. Not further pipeline will be executed after this one for specified Action Marker.
        /// For order specification see <see cref="ISequenceHandler"/>
        /// </summary>
        /// <typeparam name="TActionMarker">Action interface</typeparam>
        public static IPipelineConfigurator UseSequenceMultiHandler<TActionMarker>(this IPipelineConfigurator config)
            where TActionMarker : IActionMarker
        {
            return config.Use<MultiHandlerSequenceExecutionMiddleware, TActionMarker>();
        }
    }
}
