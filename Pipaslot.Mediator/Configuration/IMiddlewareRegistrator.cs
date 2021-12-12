using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Configuration
{
    public interface IMiddlewareRegistrator
    {
        /// <summary>
        /// Register middleware in pipeline for all actions
        /// </summary>
        IConditionalPipelineConfigurator Use<TMiddleware>() where TMiddleware : IMediatorMiddleware;
    }
}
