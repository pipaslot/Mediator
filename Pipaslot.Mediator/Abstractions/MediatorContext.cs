using System;
using System.Collections.Generic;
using System.Text;

namespace Pipaslot.Mediator.Abstractions
{
    public class MediatorContext
    {
        public List<string> ErrorMessages { get; } = new List<string>();
        public List<object> Results { get; } = new List<object>(1);
    }
}
