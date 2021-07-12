using Pipaslot.Mediator.Abstractions;
using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator
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
            var pipelines = _serviceProvider.GetServices<PipelineDefinition>()
                .ToArray()
                .Where(d => d.MarkerType == null || d.MarkerType.IsAssignableFrom(requestType))
                .Select(d => (IMediatorMiddleware)_serviceProvider.GetRequiredService(d.PipelineType));

            foreach (var pipeline in pipelines)
            {
                yield return pipeline;
                if (pipeline is IExecutionMiddleware)
                {
                    yield break;
                }
            }

            yield return new SingleHandlerExecutionMiddleware(this);
        }
    }
}
