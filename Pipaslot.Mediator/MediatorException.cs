using Pipaslot.Mediator.Configuration;
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
            return new MediatorException($"Multiple handlers were registered for the same action. Only one handler was expected. Remove one from defined type: {string.Join(" OR ", handlers)}");
        }

        public static MediatorException CreateForCanNotCombineHandlers(object[] handlers)
        {
            return new MediatorException($"Multiple handlers were registered for the same action. Can not combine handlers with interfaces {nameof(ISequenceHandler)} or {nameof(IConcurrentHandler)} or without if any if these two interfaces. Please check handlers: {string.Join(", ", handlers)}");
        }

        public static MediatorException CreateForNoActionRegistered()
        {
            return new MediatorException($"No action marker assembly was registered. Use {nameof(MediatorConfigurator.AddActionsFromAssembly)} during pipeline setup");
        }

        public static MediatorException CreateForInvalidHandlers(params string[] errors)
        {
            var ex = new MediatorException($"Invalid handle configuration. For more details see Data property.");
            var i = 1;
            foreach (var error in errors)
            {
                ex.Data["Error:" + i] = error;
                i++;
            }

            return ex;
        }

    }
}
