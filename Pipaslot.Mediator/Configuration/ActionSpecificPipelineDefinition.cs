using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator.Configuration
{
    internal class ActionSpecificPipelineDefinition : IConditionalPipelineConfigurator
    {
        private readonly PipelineConfigurator _configurator;
        private readonly List<Type> _middlewares = new ();

        public IReadOnlyList<Type> MiddlewareTypes => _middlewares;
        /// <summary>
        /// Unique pipeline identifier
        /// </summary>
        public string Identifier { get; }

        /// <summary>
        /// Condition deciding wheather the pipeline will be applied or not
        /// </summary>
        public Func<Type, bool> Condition { get; }

        public ActionSpecificPipelineDefinition(PipelineConfigurator configurator, string identifier, Func<Type, bool> contition)
        {
            _configurator = configurator;
            Identifier = identifier;
            Condition = contition;
        }

        public IConditionalPipelineConfigurator Use<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware
        {
            var type = typeof(TMiddleware);
            _middlewares.Add(type);
            _configurator.RegisterMiddleware(type, lifetime);
            return this;
        }

        public IConditionalPipelineConfigurator AddPipeline(string identifier, Func<Type, bool> actionCondition)
        {
            return _configurator.AddPipeline(identifier, actionCondition);
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
