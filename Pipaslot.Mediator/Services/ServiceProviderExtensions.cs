using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;

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
            var mediatorActionType = typeof(IMediatorAction);
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
        public static object[] GetRequestHandlers(this IServiceProvider serviceProvider, Type? requestType, Type? responseType)
        {
            if (requestType == null || responseType == null)
            {
                return new object[0];
            }
            var mediatorHandlerType = typeof(IMediatorHandler<,>);
            var handlerType = mediatorHandlerType.MakeGenericType(requestType, responseType);
            return serviceProvider.GetServices(handlerType)
                .Where(h => h != null)
                // ReSharper disable once RedundantEnumerableCastCall
                .Cast<object>()
                .ToArray();
        }

        private static bool IsRequestInterface(Type type, Type responseType)
        {
            if (type.IsInterface)
            {
                var mediatorActionType = typeof(IMediatorAction<>).MakeGenericType(responseType);
                return mediatorActionType.IsAssignableFrom(type);
            }
            return false;
        }

        public static IExecutionMiddleware GetExecutiveMiddleware(this IServiceProvider serviceProvider, Type actionType)
        {
            var pipeline = serviceProvider.GetPipeline(actionType).Last();
            if (pipeline is IExecutionMiddleware ep)
            {
                return ep;
            }
            throw new MediatorException("Executive pipeline not found");//This should never happen as GetMessagePipelines always returns last pipeline as executive
        }

        public static IEnumerable<IMediatorMiddleware> GetPipeline(this IServiceProvider serviceProvider, Type actionType)
        {
            var actionSpecificPipelineDefinitions = serviceProvider.GetServices<ActionSpecificPipelineDefinition>();
            var actionSpecificPipeline = actionSpecificPipelineDefinitions
                .Where(p => p.Condition(actionType))
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

            yield return serviceProvider.GetRequiredService<IExecutionMiddleware>();
        }
    }
}
