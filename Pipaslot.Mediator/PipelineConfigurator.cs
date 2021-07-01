using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pipaslot.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

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

        public IPipelineConfigurator AddMarkersFromAssemblyOf<T>()
        {
            return AddMarkersFromAssemblyOf(typeof(T).Assembly);
        }

        public IPipelineConfigurator AddMarkersFromAssemblyOf(params Assembly[] assemblies)
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
                typeof(IRequestHandler<,>),
                typeof(IMessageHandler<>)
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

        public IPipelineConfigurator Use<TPipeline>()
            where TPipeline : IMediatorMiddleware
        {
            return RegisterMidlewares(typeof(TPipeline));
        }
        public IPipelineConfigurator Use<TPipeline, TActionMarker>()
            where TPipeline : IMediatorMiddleware
            where TActionMarker : IActionMarker
        {
            return RegisterMidlewares(typeof(TPipeline), typeof(TActionMarker));
        }

        private IPipelineConfigurator RegisterMidlewares(Type pipeline, Type? markerType = null)
        {
            _services.AddSingleton(new PipelineDefinition(pipeline, markerType));
            _services.AddScoped(pipeline);

            return this;
        }
    }
}
