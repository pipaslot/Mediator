using Pipaslot.Mediator.Middlewares;
using System;

namespace Pipaslot.Mediator
{
    public class MediatorExecutionException : Exception
    {
        /// <summary>
        /// Response containing all information gathered from Mediator execution
        /// </summary>
        public IMediatorResponse Response { get; }

        public MediatorExecutionException(string message, MediatorContext context) : base(message)
        {
            Response = new MediatorResponse(false, context.Results, context.ErrorMessagesDistincted);
        }

        public MediatorExecutionException(string message, IMediatorResponse response) : base(message)
        {
            Response = response;
        }

        public MediatorExecutionException(IMediatorResponse response) : base(string.Join("; ", response.ErrorMessages))
        {
            Response = response;
        }
    }
}
