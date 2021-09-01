using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator
{
    internal class DefaultPipelineDefinition : IConditionalPipelineConfigurator
    {
        private readonly PipelineConfigurator _configurator;
        private List<Type> _middlewares = new List<Type>();

        public IReadOnlyList<Type> MiddlewareTypes => _middlewares;

        public DefaultPipelineDefinition(PipelineConfigurator configurator)
        {
            _configurator = configurator;
        }

        public IConditionalPipelineConfigurator Use<TMiddleware>() where TMiddleware : IMediatorMiddleware
        {
            var type = typeof(TMiddleware);
            _middlewares.Add(type);
            _configurator.RegisterMiddleware(type);
            return this;
        }

        public IConditionalPipelineConfigurator AddPipeline<TActionMarker>()
        {
            return _configurator.AddPipeline<TActionMarker>();
        }

        public IConditionalPipelineConfigurator AddDefaultPipeline()
        {
            return _configurator.AddDefaultPipeline();
        }
    }
}
