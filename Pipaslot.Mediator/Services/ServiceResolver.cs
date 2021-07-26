using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using Pipaslot.Mediator.Middlewares;

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
            var handlerType = typeof(IMessageHandler<>).MakeGenericType(messageType);
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
            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
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
            var actionSpecificPipeline = _serviceProvider.GetServices<ActionSpecificPipelineDefinition>()
                .Where(p => p.MarkerType != null && p.MarkerType.IsAssignableFrom(requestType))
                .FirstOrDefault();
            if (actionSpecificPipeline != null)
            {
                var actionSpecificPipelineMiddlewares = actionSpecificPipeline.MiddlewareTypes.Select(m => (IMediatorMiddleware)_serviceProvider.GetRequiredService(m));
                return GetMiddlewaresWithLastExecutive(actionSpecificPipelineMiddlewares);
            }

            var defaultPipeline = _serviceProvider.GetServices<ActionSpecificPipelineDefinition>()
                .Where(p => p.MarkerType == null)
                .FirstOrDefault();
            if (defaultPipeline != null)
            {
                var defaultPipelineMiddlewares = defaultPipeline.MiddlewareTypes.Select(m => (IMediatorMiddleware)_serviceProvider.GetRequiredService(m));
                return GetMiddlewaresWithLastExecutive(defaultPipelineMiddlewares);
            }

            var defaultMiddlewares = _serviceProvider.GetServices<PipelineDefinition>()
                .ToArray()
                .Where(d => d.MarkerType == null || d.MarkerType.IsAssignableFrom(requestType))
                .Select(d => (IMediatorMiddleware)_serviceProvider.GetRequiredService(d.PipelineType));

            return GetMiddlewaresWithLastExecutive(defaultMiddlewares);
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
