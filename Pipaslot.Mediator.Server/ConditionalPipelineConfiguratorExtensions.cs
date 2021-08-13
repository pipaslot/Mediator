namespace Pipaslot.Mediator.Server
{
    public static class ConditionalPipelineConfiguratorExtensions
    {
        /// <summary>
        /// Register exception logging middleware writting all error into ILogger
        /// </summary>
        public static IConditionalPipelineConfigurator UseExceptionLogging(this IConditionalPipelineConfigurator configurator)
        {
            configurator.Use<MediatorExceptionLoggingMiddleware>();
            return configurator;
        }
    }
}
