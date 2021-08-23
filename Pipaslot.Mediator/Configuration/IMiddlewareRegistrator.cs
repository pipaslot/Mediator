using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator
{
    public interface IMiddlewareRegistrator
    {
        /// <summary>
        /// Register middleware in pipeline for all actions
        /// </summary>
        IConditionalPipelineConfigurator Use<TMiddleware>() where TMiddleware : IMediatorMiddleware;
    }
}
