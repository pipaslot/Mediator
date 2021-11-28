using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pipaslot.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator.Configuration
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

        public IConditionalPipelineConfigurator AddPipeline<TActionMarker>()
        {
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
            var pipeline = new DefaultPipelineDefinition(this);
            var existingPipelineDescriptor = _services.FirstOrDefault((ServiceDescriptor d) => d.ServiceType == typeof(DefaultPipelineDefinition));
            if (existingPipelineDescriptor != null)
            {
                _services.Remove(existingPipelineDescriptor);
            }

            _services.AddSingleton(pipeline);

            return pipeline;
        }

        internal void RegisterMiddleware(Type middlewareType, ServiceLifetime lifetime)
        {
            _services.Add(new ServiceDescriptor(middlewareType, middlewareType, lifetime));
        }
    }
}
