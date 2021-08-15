using System;
using System.Collections.Generic;
using System.Linq;

namespace Pipaslot.Mediator.Middlewares
{
    public class MediatorContext
    {
        public List<string> ErrorMessages { get; } = new List<string>();
        public List<object> Results { get; } = new List<object>(1);

        internal string[] ErrorMessagesDistincted => ErrorMessages.Distinct().ToArray();
    }
}
