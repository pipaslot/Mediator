﻿using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Pipaslot.Mediator.Configuration
{
    public class MediatorConfigurator : IMediatorConfigurator, IActionTypeProvider, IMiddlewareResolver
    {
        internal readonly IServiceCollection Services;
        internal List<Assembly> ActionMarkerAssemblies { get; } = new List<Assembly>();
        private MiddlewareCollection _middlewares;
        private List<(Func<IMediatorAction, bool> Condition,MiddlewareCollection Middlewares)> _pipelines = new ();

        public MediatorConfigurator(IServiceCollection services)
        {
            Services = services;
            _middlewares = new MiddlewareCollection(services);
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
            var types = assemblies.SelectMany(a => a.GetTypes());
            Services.RegisterHandlers(types, serviceLifetime);
            return this;
        } 

        public IMiddlewareRegistrator Use<TMiddleware>(Action<IServiceCollection> setupDependencies, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware
        {
            return _middlewares.Use<TMiddleware>(setupDependencies, lifetime);
        }

        public IMiddlewareRegistrator Use<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware
        {
            return _middlewares.Use<TMiddleware>(lifetime);
        }

        public IMiddlewareRegistrator UseWhen(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            _middlewares.UseWhen(condition, subMiddlewares);
            return this;
        }

        public IMediatorConfigurator AddPipeline(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            var collection = new MiddlewareCollection(Services);
            subMiddlewares(collection);
            _pipelines.Add((condition, collection));
            return this;
        }

        public Type[] GetMessageActionTypes()
        {
            var types = GetAllPotentialActionTypes();
            return FilterAssignableToMessage(types);
        }

        public Type[] GetRequestActionTypes()
        {
            var types = GetAllPotentialActionTypes();
            return FilterAssignableToRequest(types);
        }

        private IEnumerable<Type> GetAllPotentialActionTypes()
        {
            return ActionMarkerAssemblies
                .SelectMany(s => s.GetTypes());
        }

        public IEnumerable<Type> GetMiddlewares(IMediatorAction action)
        {
            var pipelines = _pipelines
                .Where(p=>p.Condition(action))
                .ToArray();
            if(pipelines.Length > 1)
            {
                throw MediatorException.TooManyPipelines(action);
            }
            else if (pipelines.Length == 1)
            {
                return pipelines.First().Middlewares.GetMiddlewares(action);
            }
            return _middlewares.GetMiddlewares(action);
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
