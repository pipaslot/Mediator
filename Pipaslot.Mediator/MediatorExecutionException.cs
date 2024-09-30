using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator
{
    /// <summary>
    /// Exception related to mediator execution, middleware processing and unexpected status during execution.
    /// </summary>
    public class MediatorExecutionException : MediatorException
    {
        /// <summary>
        /// Response containing all information gathered from Mediator execution
        /// </summary>
        public IMediatorResponse Response { get; }
        
        public MediatorExecutionException(string message, MediatorContext? context) : base(message)
        {
            Response = new MediatorResponse(false, context?.Results ?? Array.Empty<object>());
        }

        public MediatorExecutionException(string message, IMediatorResponse response) : base($"{message} Errors: ['{GetErrors(response.Results)}']")
        {
            Response = response;
        }

        public MediatorExecutionException(IMediatorResponse response) : base(GetErrors(response.Results))
        {
            Response = response;
        }

        public static MediatorExecutionException CreateForUnhandledError(MediatorContext context)
        {
            return CreateForUnhandledError($"'{GetErrors(context.Results)}'", context);
        }

        public static MediatorExecutionException CreateForUnhandledError(string errors, MediatorContext context)
        {
            return new MediatorExecutionException($"Handler or middlewares set the ExecutionStatus to {ExecutionStatus.Failed}. To prevent this exception, user methods Mediator.Dispatch or Mediator.Execute instead. Error messages: [{errors}]", context);
        }

        internal static MediatorExecutionException CreateForMissingResult(MediatorContext context, Type type)
        {
            return new MediatorExecutionException($"Expected result type '{type}' was missing in result collection. Ensure that executed action has its handler.", context);
        }

        internal static MediatorExecutionException CreateForNoHandler(Type? type, MediatorContext? context = null)
        {
            return new MediatorExecutionException("No handler was found for " + type, context);
        }

        private static string GetErrors(IReadOnlyCollection<object> results)
        {
            var errors = results
                    .Where(r => r is Notification)
                    .Cast<Notification>()
                    .Where(n => n.Type.IsError())
                    .Select(n => n.Content);
            return string.Join("; ", errors);
        }
    }
}
