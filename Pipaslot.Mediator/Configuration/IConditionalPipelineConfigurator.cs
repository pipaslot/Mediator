using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Define single pipeline definition. Mulptiple pilepinec can be registered but only single one will be always applied.
    /// </summary>
    public interface IConditionalPipelineConfigurator : IPipelineRegistrator
    {
        /// <summary>
        /// Register middleware in pipeline for all actions
        /// </summary>
        IConditionalPipelineConfigurator Use<TMiddleware>() where TMiddleware : IMediatorMiddleware;
    }
}
