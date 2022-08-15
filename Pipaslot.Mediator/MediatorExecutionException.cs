using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator
{
    public class MediatorExecutionException : MediatorException
    {
        /// <summary>
        /// Response containing all information gathered from Mediator execution
        /// </summary>
        public IMediatorResponse Response { get; }

        public MediatorExecutionException(string message, MediatorContext context) : base($"{message} Errors: ['{GetErrors(context.Results)}']")
        {
            Response = new MediatorResponse(false, context.Results);
        }

        public MediatorExecutionException(string message, IMediatorResponse response) : base($"{message} Errors: ['{GetErrors(response.Results)}']")
        {
            Response = response;
        }

        public MediatorExecutionException(IMediatorResponse response) : base(GetErrors(response.Results))
        {
            Response = response;
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

        public static MediatorExecutionException CreateForUnhandledError(MediatorContext context)
        {
            return new MediatorExecutionException("An error occurred during processing.", context);
        }

        internal static MediatorExecutionException CreateForMissingResult(MediatorContext context, Type type)
        {
            return new MediatorExecutionException($"Extected result type '{type}' was missing in result collection. Ensure that executed action has its handler.", context);
        }
    }
}
