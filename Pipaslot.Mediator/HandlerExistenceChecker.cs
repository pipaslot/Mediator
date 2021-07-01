using System;
using System.Collections.Generic;
using System.Linq;
using Pipaslot.Mediator.Abstractions;

namespace Pipaslot.Mediator
{
    public class HandlerExistenceChecker
    {
        /// <summary>
        /// We need to ignore handlers on less generic type. For example once command is catch, then we do not expect that generic IHandler will process that command as well.
        /// </summary>
        private readonly HashSet<Type> _alreadyVerified = new HashSet<Type>();
        private readonly ServiceResolver _handlerResolver;
        private readonly PipelineConfigurator _configurator;

        public HandlerExistenceChecker(ServiceResolver handlerResolver, PipelineConfigurator configurator)
        {
            _handlerResolver = handlerResolver;
            _configurator = configurator;
        }

        public void Verify()
        {
            var assemblies = _configurator.ActionMarkerAssemblies;
            if(assemblies.Count == 0)
            {
                throw new Exception($"No action marker assembly was registered. Use {nameof(PipelineConfigurator.AddMarkersFromAssemblyOf)} during pipeline setup");
            }
            var types = assemblies.SelectMany(s => s.GetTypes());
            VerifyMessages(types);
            VerifyRequests(types);
        }

        private void VerifyMessages(IEnumerable<Type> types)
        {
            var subjectName = typeof(IMessage).Name;
            var queryTypes = GenericHelpers.FilterAssignableToMessage(types);
            foreach (var subject in queryTypes)
            {
                if (_alreadyVerified.Contains(subject))
                {
                    continue;
                }

                var handlers = _handlerResolver.GetMessageHandlers(subject).ToArray();
                var middleware = _handlerResolver.GetExecutiveMiddleware(subject);
                VerifyHandlerCount(middleware, handlers, subject, subjectName);
                _alreadyVerified.Add(subject);
            }
        }

        private void VerifyRequests(IEnumerable<Type> types)
        {
            var subjectName = typeof(IRequest<>).Name;
            var queryTypes = GenericHelpers.FilterAssignableToRequest(types);
            foreach (var subject in queryTypes)
            {
                if (_alreadyVerified.Contains(subject))
                {
                    continue;
                }
                var resultType = GenericHelpers.GetRequestResultType(subject);
                var handlers = _handlerResolver.GetRequestHandlers(subject, resultType);
                var middleware = _handlerResolver.GetExecutiveMiddleware(subject);
                VerifyHandlerCount(middleware, handlers, subject, subjectName);
                _alreadyVerified.Add(subject);
            }
        }
        private void VerifyHandlerCount(IExecutionMiddleware middleware, object[] handlers, Type subject, string subjectName)
        {
            if (handlers.Count() == 0)
            {
                throw new Exception($"No handler was registered for {subjectName} type: {subject}");
            }
            if (!middleware.ExecuteMultipleHandlers && handlers.Count() > 1)
            {
                throw new Exception($"Multiple {subjectName} handlers were registered for one {subjectName} type: {subject} with classes {string.Join(" AND ", handlers)}");
            }
        }
    }
}
