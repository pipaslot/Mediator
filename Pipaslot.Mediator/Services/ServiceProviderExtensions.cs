using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;

namespace Pipaslot.Mediator.Services
{
    internal static class ServiceProviderExtensions
    {
        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        public static object[] GetMessageHandlers(this IServiceProvider serviceProvider, Type? messageType, ActionToHandlerBindingType bindingType)
        {
            if (messageType == null)
            {
                return new object[0];
            }
            var mediatorActionType = typeof(IMediatorAction);
            var actionTypes = bindingType == ActionToHandlerBindingType.Class
                ? new[] { messageType }
                : bindingType == ActionToHandlerBindingType.Interface
                    ? messageType
                        .GetInterfaces()
                        .Where(i => mediatorActionType.IsAssignableFrom(i))
                        .ToArray()
                    : throw new NotImplementedException($"Unknown binding type {bindingType}");

            var handlers = new List<object>();
            foreach (var actionType in actionTypes)
            {
                var handlerType = typeof(IMediatorHandler<>).MakeGenericType(actionType);
                handlers.AddRange(serviceProvider.GetServices(handlerType)
                    .Where(h => h != null)
                    // ReSharper disable once RedundantEnumerableCastCall
                    .Cast<object>()
                    .ToArray());
            }
            return handlers.ToArray();
        }

        /// <summary>
        /// Get all registered handlers from service provider
        /// </summary>
        public static object[] GetRequestHandlers(this IServiceProvider serviceProvider, Type? requestType, Type? responseType, ActionToHandlerBindingType bindingType)
        {
            if (requestType == null || responseType == null)
            {
                return new object[0];
            }
            var mediatorHandlerType = typeof(IMediatorHandler<,>);
            var actionTypes = bindingType == ActionToHandlerBindingType.Class
                ? new[] { requestType }
                : bindingType == ActionToHandlerBindingType.Interface
                    ? requestType
                        .GetInterfaces()
                        .Where(i => IsRequestInterface(i, responseType))
                        .ToArray()
                    : throw new NotImplementedException($"Unknown binding type {bindingType}");

            var handlers = new List<object>();
            foreach (var actionType in actionTypes)
            {
                var handlerType = mediatorHandlerType.MakeGenericType(actionType, responseType);
                handlers.AddRange(serviceProvider.GetServices(handlerType)
                    .Where(h => h != null)
                    // ReSharper disable once RedundantEnumerableCastCall
                    .Cast<object>()
                    .ToArray());
            }
            return handlers.ToArray();
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

            yield return serviceProvider.GetRequiredService<IExecutionMiddleware>();
        }
    }
}
