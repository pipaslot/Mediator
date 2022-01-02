using System;
using System.Collections.Generic;
using System.Linq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Services
{
    public class HandlerExistenceChecker : IHandlerExistenceChecker
    {
        /// <summary>
        /// We need to ignore handlers on less generic type. For example once command is catch, then we do not expect that generic IHandler will process that command as well.
        /// </summary>
        private readonly HashSet<Type> _alreadyVerified = new();
        private readonly IServiceProvider _serviceProvider;
        private readonly IActionTypeProvider _actionTypeProvider;
        private readonly List<string> _errors = new();

        public HandlerExistenceChecker(IServiceProvider serviceProvider, IActionTypeProvider actionTypeProvider)
        {
            _serviceProvider = serviceProvider;
            _actionTypeProvider = actionTypeProvider;
        }

        public void Verify()
        {
            var messageTypes = _actionTypeProvider.GetMessageActionTypes();
            var requestTypes = _actionTypeProvider.GetRequestActionTypes();
            var typeCount = messageTypes.Count() + requestTypes.Count();
            if (typeCount == 0)
            {
                throw MediatorException.CreateForNoActionRegistered();
            }

            VerifyMessages(messageTypes);
            VerifyRequests(requestTypes);
            if (_errors.Any())
            {
                throw MediatorException.CreateForInvalidHandlers(_errors.ToArray());
            }
        }

        private void VerifyMessages(IEnumerable<Type> queryTypes)
        {
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
            foreach (var subject in types)
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
    }
}
