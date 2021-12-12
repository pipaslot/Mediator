using System;

namespace Pipaslot.Mediator
{
    public class MediatorException : Exception
    {
        public MediatorException(string message) : base(message)
        {
        }

        public static MediatorException CreateForNoHandler(Type? type)
        {
            return new MediatorException("No handler was found for " + type);
        }

        public static MediatorException CreateForDuplicateHandlers(object[] handlers)
        {
            return new MediatorException($"Multiple handlers were registered for the same action. Remove one from defined type: {string.Join(" OR ", handlers)}");
        }
        
    }
}
