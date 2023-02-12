using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Configuration
{
    internal class MiddlewareCollection : IMiddlewareResolver, IMiddlewareRegistrator
    {
        private readonly List<IMiddlewareResolver> _middlewareTypes = new List<IMiddlewareResolver>();
        private readonly IServiceCollection _services;

        public MiddlewareCollection(IServiceCollection services)
        {
            _services = services;
        }

        public void AddMiddleware(Type middlewareType, ServiceLifetime lifetime)
        {
            _middlewareTypes.Add(new MiddlewareDefinition(middlewareType));
            var existingDescriptor = _services.FirstOrDefault(d => d.ServiceType == middlewareType && d.ImplementationType == middlewareType);
            if (existingDescriptor != null)
            {
                if (existingDescriptor.Lifetime != lifetime)
                {
                    throw new MediatorException($"Can not register the same middleware with different ServiceLifetime. Service {middlewareType} was already registered with ServiceLifetime {existingDescriptor.Lifetime}.");
                }
            }
            else
            {
                _services.Add(new ServiceDescriptor(middlewareType, middlewareType, lifetime));
            }
        }

        public IEnumerable<Type> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
        {
            foreach (var res in _middlewareTypes)
            {
                foreach (var a in res.GetMiddlewares(action, serviceProvider))
                {
                    yield return a;
                }
            }
        }

        public IMiddlewareRegistrator Use<TMiddleware>(ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware
        {
            AddMiddleware(typeof(TMiddleware), lifetime);
            return this;
        }

        public IMiddlewareRegistrator Use<TMiddleware>(Action<IServiceCollection> setupDependencies, ServiceLifetime lifetime = ServiceLifetime.Scoped) where TMiddleware : IMediatorMiddleware
        {
            setupDependencies(_services);
            AddMiddleware(typeof(TMiddleware), lifetime);
            return this;
        }

        public IMiddlewareRegistrator UseWhen(Func<IMediatorAction, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            var config = new MiddlewareCollection(_services);
            var definition = new ConditionDefinition(condition, config);
            _middlewareTypes.Add(definition);
            subMiddlewares(config);
            return this;
        }

        public IMiddlewareRegistrator UseWhen(Func<IMediatorAction, IServiceProvider, bool> condition, Action<IMiddlewareRegistrator> subMiddlewares)
        {
            var config = new MiddlewareCollection(_services);
            var definition = new DynamicDefinition(condition, config);
            _middlewareTypes.Add(definition);
            subMiddlewares(config);
            return this;
        }

        private class MiddlewareDefinition : IMiddlewareResolver
        {
            private readonly Type _middlewareType;

            public MiddlewareDefinition(Type middlewareType)
            {
                _middlewareType = middlewareType;
            }

            public IEnumerable<Type> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
            {
                yield return _middlewareType;
            }
        }

        private class ConditionDefinition : IMiddlewareResolver
        {
            private readonly Func<IMediatorAction, bool> _condition;

            private readonly MiddlewareCollection _middlewares;

            public ConditionDefinition(Func<IMediatorAction, bool> condition, MiddlewareCollection middlewares)
            {
                _condition = condition;
                _middlewares = middlewares;
            }

            public IEnumerable<Type> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
            {
                if (_condition(action))
                {
                    foreach (var type in _middlewares.GetMiddlewares(action, serviceProvider))
                    {
                        yield return type;
                    }
                }
            }
        }

        private class DynamicDefinition : IMiddlewareResolver
        {
            private readonly Func<IMediatorAction, IServiceProvider, bool> _condition;
            private readonly MiddlewareCollection _middlewares;

            public DynamicDefinition(Func<IMediatorAction, IServiceProvider, bool> condition, MiddlewareCollection middlewares)
            {
                _condition = condition;
                _middlewares = middlewares;
            }

            public IEnumerable<Type> GetMiddlewares(IMediatorAction action, IServiceProvider serviceProvider)
            {
                if (_condition(action, serviceProvider))
                {
                    foreach (var type in _middlewares.GetMiddlewares(action, serviceProvider))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
