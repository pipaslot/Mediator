using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;
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
    }
}
