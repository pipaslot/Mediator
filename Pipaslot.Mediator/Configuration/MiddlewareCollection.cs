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

        public MiddlewareCollection AddCondition(Func<IMediatorAction, bool> condition)
        {
            var subCollection = new MiddlewareCollection(_services);
            var definition = new ConditionDefinition(condition, subCollection);
            _middlewareTypes.Add(definition);
            return subCollection;
        }

        public IEnumerable<Type> GetMiddlewares(IMediatorAction action)
        {
            foreach (var res in _middlewareTypes)
            {
                foreach (var a in res.GetMiddlewares(action))
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
            var config = AddCondition(condition);
            subMiddlewares(config);
            return this;
        }

        private class MiddlewareDefinition : IMiddlewareResolver
        {
            private Type _middlewareType;

            public MiddlewareDefinition(Type middlewareType)
            {
                _middlewareType = middlewareType;
            }

            public IEnumerable<Type> GetMiddlewares(IMediatorAction action)
            {
                yield return _middlewareType;
            }
        }

        private class ConditionDefinition : IMiddlewareResolver
        {
            private Func<IMediatorAction, bool> _condition;

            public MiddlewareCollection _middlewares;

            public ConditionDefinition(Func<IMediatorAction, bool> condition, MiddlewareCollection middlewares)
            {
                _condition = condition;
                _middlewares = middlewares;
            }

            public IEnumerable<Type> GetMiddlewares(IMediatorAction action)
            {
                if (_condition(action))
                {
                    foreach (var type in _middlewares.GetMiddlewares(action))
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}
