﻿using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator.Services
{
    internal static class ServiceProviderExtensions
    {
        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        public static object[] GetMessageHandlers(this IServiceProvider serviceProvider, Type? messageType)
        {
            if (messageType == null)
            {
                return new object[0];
            }
            var handlerType = typeof(IMediatorHandler<>).MakeGenericType(messageType);
            return serviceProvider.GetServices(handlerType)
                .Where(h => h != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<object>()
                .ToArray();
        }

        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        public static object[] GetRequestHandlers<TResponse>(this IServiceProvider serviceProvider, Type? requestType)
        {
            return serviceProvider.GetRequestHandlers(requestType, typeof(TResponse));
        }


        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        public static object[] GetRequestHandlers(this IServiceProvider serviceProvider, Type? requestType, Type? responseType)
        {
            if (requestType == null || responseType == null)
            {
                return new object[0];
            }
            var handlerType = typeof(IMediatorHandler<,>).MakeGenericType(requestType, responseType);
            return serviceProvider.GetServices(handlerType)
                .Where(h => h != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<object>()
                .ToArray();
        }

        public static IExecutionMiddleware GetExecutiveMiddleware(this IServiceProvider serviceProvider, Type requestType)
        {
            var pipeline = serviceProvider.GetPipeline(requestType).Last();
            if (pipeline is IExecutionMiddleware ep)
            {
                return ep;
            }
            throw new Exception("Executive pipeline not found");//This should never happen as GetMessagePipelines always returns last pipeline as executive
        }

        public static IEnumerable<IMediatorMiddleware> GetPipeline(this IServiceProvider serviceProvider, Type requestType)
        {
            var actionSpecificPipelineDefinitions = serviceProvider.GetServices<ActionSpecificPipelineDefinition>();
            var actionSpecificPipeline = actionSpecificPipelineDefinitions
                .Where(p => p.MarkerType.IsAssignableFrom(requestType))
                .FirstOrDefault();
            if (actionSpecificPipeline != null)
            {
                var actionSpecificPipelineMiddlewares = actionSpecificPipeline.MiddlewareTypes.Select(m => (IMediatorMiddleware)serviceProvider.GetRequiredService(m));
                return serviceProvider.GetMiddlewaresWithLastExecutive(actionSpecificPipelineMiddlewares);
            }

            var defaultPipeline = serviceProvider.GetServices<DefaultPipelineDefinition>()
                .FirstOrDefault();
            if (defaultPipeline != null)
            {
                var defaultPipelineMiddlewares = defaultPipeline.MiddlewareTypes.Select(m => (IMediatorMiddleware)serviceProvider.GetRequiredService(m));
                return serviceProvider.GetMiddlewaresWithLastExecutive(defaultPipelineMiddlewares);
            }

            return serviceProvider.GetMiddlewaresWithLastExecutive(new IMediatorMiddleware[0]);
        }

        private static IEnumerable<IMediatorMiddleware> GetMiddlewaresWithLastExecutive(this IServiceProvider serviceProvider, IEnumerable<IMediatorMiddleware> pipeline)
        {
            foreach (var middleware in pipeline)
            {
                yield return middleware;
                if (middleware is IExecutionMiddleware)
                {
                    yield break;
                }
            }

            yield return new SingleHandlerExecutionMiddleware(serviceProvider);
        }
    }
}