using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pipaslot.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator
{
    public class PipelineConfigurator : IPipelineConfigurator
    {
        private readonly IServiceCollection _services;
        public List<Assembly> ActionMarkerAssemblies { get; } = new List<Assembly>();

        public PipelineConfigurator(IServiceCollection services)
        {
            _services = services;
        }

        public IPipelineConfigurator AddActionsFromAssemblyOf<T>()
        {
            return AddActionsFromAssembly(typeof(T).Assembly);
        }

        public IPipelineConfigurator AddActionsFromAssembly(params Assembly[] assemblies)
        {
            ActionMarkerAssemblies.AddRange(assemblies);
            return this;
        }

        public IPipelineConfigurator AddHandlersFromAssemblyOf<T>()
        {
            return AddHandlersFromAssembly(typeof(T).Assembly);
        }

        public IPipelineConfigurator AddHandlersFromAssembly(params Assembly[] assemblies)
        {
            var handlerTypes = new[]
            {
                typeof(IMediatorHandler<,>),
                typeof(IMediatorHandler<>)
            };
            var types = assemblies
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && handlerTypes.Contains(i.GetGenericTypeDefinition())))
                .Select(t => new
                {
                    Type = t,
                    Interfaces = t.GetInterfaces()
                        .Where(i => i.IsGenericType && handlerTypes.Contains(i.GetGenericTypeDefinition()))
                });
            foreach (var pair in types)
            {
                foreach (var iface in pair.Interfaces)
                {
                    _services.AddTransient(iface, pair.Type);
                }
            }
            return this;
        }

        [Obsolete("Pipeline definition was replaced by .AddDefaultPipeline(...).Use<TMiddleware>()")]
        public IPipelineConfigurator Use<TPipeline>()
            where TPipeline : IMediatorMiddleware
        {
            return RegisterMidlewares(typeof(TPipeline));
        }

        [Obsolete("Pipeline definition was replaced by .AddPipeline<TActionMarker>(...).Use<TMiddleware>()")]
        public IPipelineConfigurator Use<TPipeline, TActionMarker>()
            where TPipeline : IMediatorMiddleware
            where TActionMarker : IMediatorAction
        {
            return RegisterMidlewares(typeof(TPipeline), typeof(TActionMarker));
        }
        [Obsolete]
        private IPipelineConfigurator RegisterMidlewares(Type pipeline, Type? markerType = null)
        {
            _services.AddSingleton(new PipelineDefinition(pipeline, markerType));
            _services.AddScoped(pipeline);

            return this;
        }

        public IConditionalPipelineConfigurator AddPipeline<TActionMarker>()
        {
            if (IsDefaultPipelineRegistered())
            {
                throw new MediatorConfigurationException($"Can not use {nameof(IPipelineRegistrator.AddPipeline)} after {nameof(IPipelineRegistrator.AddDefaultPipeline)}. Check your startup.cs and move all {nameof(IPipelineRegistrator.AddPipeline)} calls before {nameof(IPipelineRegistrator.AddDefaultPipeline)}.");
            }
            var markerType = typeof(TActionMarker);
            var pipeline = new ActionSpecificPipelineDefinition(this, markerType);
            var existingPipelineDescriptor = _services.FirstOrDefault((ServiceDescriptor d) => d.ServiceType == typeof(ActionSpecificPipelineDefinition) && ((ActionSpecificPipelineDefinition)d.ImplementationInstance).MarkerType == markerType);
            if(existingPipelineDescriptor != null)
            {
                _services.Remove(existingPipelineDescriptor);
            }
            _services.AddSingleton(pipeline);
            return pipeline;
        }

        public IConditionalPipelineConfigurator AddDefaultPipeline()
        {
            var pipeline = new ActionSpecificPipelineDefinition(this, null);

            if (IsDefaultPipelineRegistered())
            {
                throw new MediatorConfigurationException($"Default pipeline is already registered. Check your startup.cs and ensure that method {nameof(IPipelineRegistrator.AddDefaultPipeline)} is used only once.");
            }
            _services.AddSingleton(pipeline);

            return pipeline;
        }

        internal void RegisterMiddleware(Type middlewareType)
        {
            _services.AddScoped(middlewareType);
        }

        internal bool IsDefaultPipelineRegistered()
        {
            return _services.Any((ServiceDescriptor d) =>
                d.ServiceType == typeof(ActionSpecificPipelineDefinition)
            && ((ActionSpecificPipelineDefinition)d.ImplementationInstance).MarkerType == null);
        }
    }
}
