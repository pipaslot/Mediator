using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Services
{
    public class ServiceResolver
    {
        private readonly IServiceProvider _serviceProvider;

        public ServiceResolver(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        public object[] GetMessageHandlers(Type? messageType)
        {
            if (messageType == null)
            {
                return new object[0];
            }
            var handlerType = typeof(IMediatorHandler<>).MakeGenericType(messageType);
            return _serviceProvider.GetServices(handlerType)
                .Where(h => h != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<object>()
                .ToArray();
        }

        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        public object[] GetRequestHandlers<TResponse>(Type? requestType)
        {
            return GetRequestHandlers(requestType, typeof(TResponse));
        }


        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        public object[] GetRequestHandlers(Type? requestType, Type? responseType)
        {
            if (requestType == null || responseType == null)
            {
                return new object[0];
            }
            var handlerType = typeof(IMediatorHandler<,>).MakeGenericType(requestType, responseType);
            return _serviceProvider.GetServices(handlerType)
                .Where(h => h != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<object>()
                .ToArray();
        }

        public IExecutionMiddleware GetExecutiveMiddleware(Type requestType)
        {
            var pipeline = GetPipeline(requestType).Last();
            if (pipeline is IExecutionMiddleware ep)
            {
                return ep;
            }
            throw new Exception("Executive pipeline not found");//This should never happen as GetMessagePipelines always returns last pipeline as executive
        }

        public IEnumerable<IMediatorMiddleware> GetPipeline(Type requestType)
        {
            var actionSpecificPipelineDefinitions = _serviceProvider.GetServices<ActionSpecificPipelineDefinition>();
            var actionSpecificPipeline = actionSpecificPipelineDefinitions
                .Where(p => p.MarkerType.IsAssignableFrom(requestType))
                .FirstOrDefault();
            if (actionSpecificPipeline != null)
            {
                var actionSpecificPipelineMiddlewares = actionSpecificPipeline.MiddlewareTypes.Select(m => (IMediatorMiddleware)_serviceProvider.GetRequiredService(m));
                return GetMiddlewaresWithLastExecutive(actionSpecificPipelineMiddlewares);
            }

            var defaultPipeline = _serviceProvider.GetServices<DefaultPipelineDefinition>()
                .FirstOrDefault();
            if (defaultPipeline != null)
            {
                var defaultPipelineMiddlewares = defaultPipeline.MiddlewareTypes.Select(m => (IMediatorMiddleware)_serviceProvider.GetRequiredService(m));
                return GetMiddlewaresWithLastExecutive(defaultPipelineMiddlewares);
            }

            return GetMiddlewaresWithLastExecutive(new IMediatorMiddleware[0]);
        }

        private IEnumerable<IMediatorMiddleware> GetMiddlewaresWithLastExecutive(IEnumerable<IMediatorMiddleware> pipeline)
        {
            foreach (var middleware in pipeline)
            {
                yield return middleware;
                if (middleware is IExecutionMiddleware)
                {
                    yield break;
                }
            }

            yield return new SingleHandlerExecutionMiddleware(this);
        }
    }
}
