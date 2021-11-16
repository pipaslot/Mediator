using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;

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

        [Obsolete("Will be removed in version 4.0.0")]
        public MediatorExecutionException(string message) : base(message)
        {
        }

        [Obsolete("Will be removed in version 4.0.0")]
        public MediatorExecutionException(ICollection<string> messages) : base(string.Join("; ", messages))
        {
        }
    }
}
