using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator
{
    public static class IConditionalPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Middleware running all handlers concurrently. Not further middleware will be executed after this one.
        /// </summary>
        public static IConditionalPipelineConfigurator UseConcurrentMultiHandler(this IConditionalPipelineConfigurator config)
        {
            return config.Use<MultiHandlerConcurrentExecutionMiddleware>();
        }

        /// <summary>
        /// Pipeline running all handlers in sequence one by one. Not further middleware will be executed after this one.
        /// For order specification see <see cref="ISequenceHandler"/>
        /// </summary>
        public static IConditionalPipelineConfigurator UseSequenceMultiHandler(this IConditionalPipelineConfigurator config)
        {
            return config.Use<MultiHandlerSequenceExecutionMiddleware>();
        }
    }
}
