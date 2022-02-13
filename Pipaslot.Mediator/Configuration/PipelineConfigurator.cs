using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Pipaslot.Mediator.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Pipaslot.Mediator.Configuration
{
    public class PipelineConfigurator : IMediatorConfigurator, IActionTypeProvider
    {
        internal readonly IServiceCollection Services;
        public List<Assembly> ActionMarkerAssemblies { get; } = new List<Assembly>();

        public PipelineConfigurator(IServiceCollection services)
        {
            Services = services;
        }

        public IMediatorConfigurator AddActionsFromAssemblyOf<T>()
        {
            return AddActionsFromAssembly(typeof(T).Assembly);
        }

        public IMediatorConfigurator AddActionsFromAssembly(params Assembly[] assemblies)
        {
            ActionMarkerAssemblies.AddRange(assemblies);
            return this;
        }

        public IMediatorConfigurator AddHandlersFromAssemblyOf<T>(ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            return RegisterHandlersFromAssembly(new[] { typeof(T).Assembly }, serviceLifetime);
        }

        public IMediatorConfigurator AddHandlersFromAssembly(params Assembly[] assemblies)
        {
            return RegisterHandlersFromAssembly(assemblies);
        }

        private IMediatorConfigurator RegisterHandlersFromAssembly(Assembly[] assemblies, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
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
                    var item = new ServiceDescriptor(iface, pair.Type, serviceLifetime);
                    Services.Add(item);
                }
            }
            return this;
        }
        public IConditionalPipelineConfigurator AddPipeline<TActionMarker>()
        {
            var markerType = typeof(TActionMarker);
            return AddPipeline(markerType.ToString(), actionType => markerType.IsAssignableFrom(actionType));
        }

        public IConditionalPipelineConfigurator AddPipeline(string identifier, Func<Type, bool> actionCondition)
        {
            var pipeline = new ActionSpecificPipelineDefinition(this, identifier, actionCondition);
            var existingPipelineDescriptor = Services.FirstOrDefault((ServiceDescriptor d) =>
                    d.ServiceType == typeof(ActionSpecificPipelineDefinition)
                && ((ActionSpecificPipelineDefinition)d.ImplementationInstance).Identifier == identifier);
            if (existingPipelineDescriptor != null)
            {
                Services.Remove(existingPipelineDescriptor);
            }
            Services.AddSingleton(pipeline);
            return pipeline;
        }

        public IConditionalPipelineConfigurator AddDefaultPipeline()
        {
            var pipeline = new DefaultPipelineDefinition(this);
            var existingPipelineDescriptor = Services.FirstOrDefault((ServiceDescriptor d) => d.ServiceType == typeof(DefaultPipelineDefinition));
            if (existingPipelineDescriptor != null)
            {
                Services.Remove(existingPipelineDescriptor);
            }

            Services.AddSingleton(pipeline);

            return pipeline;
        }

        internal void RegisterMiddleware(Type middlewareType, ServiceLifetime lifetime)
        {
            var existingDescriptor = Services.FirstOrDefault(d => d.ServiceType == middlewareType && d.ImplementationType == middlewareType);
            if (existingDescriptor != null)
            {
                if (existingDescriptor.Lifetime != lifetime)
                {
                    throw new MediatorException($"Can not register the same middleware with different ServiceLifetime. Service {middlewareType} was already registered with ServiceLifetime {existingDescriptor.Lifetime}.");
                }
            }
            else
            {
                Services.Add(new ServiceDescriptor(middlewareType, middlewareType, lifetime));
            }
        }

        public Type[] GetMessageActionTypes()
        {
            var types = ActionMarkerAssemblies.SelectMany(s => s.GetTypes());
            return FilterAssignableToMessage(types);
        }

        public Type[] GetRequestActionTypes()
        {
            var types = ActionMarkerAssemblies.SelectMany(s => s.GetTypes());
            return FilterAssignableToRequest(types);
        }

        internal static Type[] FilterAssignableToRequest(IEnumerable<Type> types)
        {
            var genericRequestType = typeof(IMediatorAction<>);
            return types
                .Where(t => t.IsClass
                        && !t.IsAbstract
                        && !t.IsInterface
                        && t.GetInterfaces()
                            .Any(i => i.IsGenericType
                                    && i.GetGenericTypeDefinition() == genericRequestType)
                )
                .ToArray();
        }

        internal static Type[] FilterAssignableToMessage(IEnumerable<Type> types)
        {
            var genericRequestType = typeof(IMediatorAction<>);
            var type = typeof(IMediatorAction);
            return types
                .Where(p => p.IsClass
                            && !p.IsAbstract
                            && !p.IsInterface
                            && p.GetInterfaces().Any(i => i == type)
                            && !p.GetInterfaces()
                            .Any(i => i.IsGenericType
                                    && i.GetGenericTypeDefinition() == genericRequestType)
                 )
                .ToArray();
        }
    }
}
