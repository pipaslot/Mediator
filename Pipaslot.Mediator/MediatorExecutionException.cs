using Pipaslot.Mediator.Middlewares;
using System;

namespace Pipaslot.Mediator
{
    public class MediatorExecutionException : MediatorException
    {
        /// <summary>
        /// Response containing all information gathered from Mediator execution
        /// </summary>
        public IMediatorResponse Response { get; }

        public MediatorExecutionException(string message, MediatorContext context) : base($"{message} Errors: ['{string.Join("; ", context.UniqueErrorMessages)}']")
        {
            Response = new MediatorResponse(false, context.Results, context.UniqueErrorMessages);
        }

        public MediatorExecutionException(string message, IMediatorResponse response) : base($"{message} Errors: ['{string.Join("; ", response.ErrorMessages)}']")
        {
            Response = response;
        }

        public MediatorExecutionException(IMediatorResponse response) : base(string.Join("; ", response.ErrorMessages))
        {
            Response = response;
        }

        public static MediatorExecutionException CreateForUnhandledError(MediatorContext context)
        {
            return new MediatorExecutionException("An error occurred during processing.", context);
        }

        internal static Exception CreateForMissingResult(MediatorContext context, Type type)
        {
            return new MediatorExecutionException($"Extected result type '{type}' was missing in result collection.", context);
        }
    }
}
