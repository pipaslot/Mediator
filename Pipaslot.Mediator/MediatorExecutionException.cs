using System;
using System.Collections.Generic;

namespace Pipaslot.Mediator
{
    public class MediatorExecutionException : Exception
    {
        public MediatorExecutionException(string message) : base( message)
        {
        }

        public MediatorExecutionException(ICollection<string> messages) : base(string.Join("; ", messages))
        {
        }
    }
}
