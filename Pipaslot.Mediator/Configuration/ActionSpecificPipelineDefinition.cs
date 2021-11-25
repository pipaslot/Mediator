using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator
{
    internal class ActionSpecificPipelineDefinition : IConditionalPipelineConfigurator
    {
        private readonly PipelineConfigurator _configurator;
        private readonly List<Type> _middlewares = new List<Type>();

        public IReadOnlyList<Type> MiddlewareTypes => _middlewares;
        /// <summary>
        /// Class or interface which needs to be implemented by Request object to be apply the pipeline for. Will be applied always if is null.
        /// </summary>
        public Type MarkerType { get; }

        public ActionSpecificPipelineDefinition(PipelineConfigurator configurator, Type markerType)
        {
            _configurator = configurator;
            MarkerType = markerType;
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
