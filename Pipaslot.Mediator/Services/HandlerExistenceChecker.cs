using System;
using System.Collections.Generic;
using System.Linq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Services
{
    public class HandlerExistenceChecker
    {
        /// <summary>
        /// We need to ignore handlers on less generic type. For example once command is catch, then we do not expect that generic IHandler will process that command as well.
        /// </summary>
        private readonly HashSet<Type> _alreadyVerified = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly PipelineConfigurator _configurator;
        private readonly List<string> _errors = new();

        public HandlerExistenceChecker(IServiceProvider serviceProvider, PipelineConfigurator configurator)
        {
            _serviceProvider = serviceProvider;
            _configurator = configurator;
        }

        public void Verify()
        {
            var assemblies = _configurator.ActionMarkerAssemblies;
            if (assemblies.Count == 0)
            {
                throw MediatorException.CreateForNoActionRegistered();
            }

            var types = assemblies.SelectMany(s => s.GetTypes());
            VerifyMessages(types);
            VerifyRequests(types);
            if (_errors.Any())
            {
                throw MediatorException.CreateForInvalidHandlers(_errors.ToArray());
            }
        }

        private void VerifyMessages(IEnumerable<Type> types)
        {
            var queryTypes = FilterAssignableToMessage(types);
            foreach (var subject in queryTypes)
            {
                if (_alreadyVerified.Contains(subject))
                {
                    continue;
                }

                var middleware = _serviceProvider.GetExecutiveMiddleware(subject);
                if (middleware is ExecutionMiddleware handlerExecution)
                {
                    var handlers = _serviceProvider.GetMessageHandlers(subject).ToArray();
                    VerifyHandlerCount(handlerExecution, handlers, subject);
                }

                _alreadyVerified.Add(subject);
            }
        }

        private void VerifyRequests(IEnumerable<Type> types)
        {
            var queryTypes = FilterAssignableToRequest(types);
            foreach (var subject in queryTypes)
            {
                if (_alreadyVerified.Contains(subject))
                {
                    continue;
                }
                var resultType = RequestGenericHelpers.GetRequestResultType(subject);
                var middleware = _serviceProvider.GetExecutiveMiddleware(subject);
                if (middleware is ExecutionMiddleware handlerExecution)
                {
                    var handlers = _serviceProvider.GetRequestHandlers(subject, resultType);
                    VerifyHandlerCount(handlerExecution, handlers, subject);
                }
                _alreadyVerified.Add(subject);
            }
        }
        private void VerifyHandlerCount(ExecutionMiddleware middleware, object[] handlers, Type subject)
        {
            if (handlers.Count() == 0)
            {
                _errors.Add(FormatNoHandlerError(subject));
            }
            if (!middleware.ExecuteMultipleHandlers && handlers.Count() > 1)
            {
                var handlerNames = handlers.Select(h => h.GetType().ToString()).ToArray();
                _errors.Add(FormatTooManyHandlersError(subject, handlerNames));
            }
        }
        internal static string FormatNoHandlerError(Type subject)
        {
            return $"No handler was registered for action: {subject}.";
        }
        internal static string FormatTooManyHandlersError(Type subject, string[] handlers)
        {
            return $"Multiple handlers were registered for one action type: {subject} with classes [{string.Join(", ", handlers)}].";
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
