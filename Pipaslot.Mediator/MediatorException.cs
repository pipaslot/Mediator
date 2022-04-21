using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using System;

namespace Pipaslot.Mediator
{
    public class MediatorException : Exception
    {
        internal static string FormatDataKey(int number)
        {
            return $"Error:{number}";
        }
        public MediatorException(string message) : base(message)
        {
        }

        public static MediatorException CreateForNoHandler(Type? type)
        {
            return new MediatorException("No handler was found for " + type);
        }

        public static MediatorException CreateForDuplicateHandlers(Type subject, object[] handlers)
        {
            return new MediatorException($"Multiple handlers were registered for one action type: {subject}. Only one handler was expected. Remove one from defined type: {string.Join(" OR ", handlers)}");
        }

        public static MediatorException CreateForCanNotCombineHandlers(Type subject, object[] handlers)
        {
            return new MediatorException($"Multiple handlers were registered for one action type: {subject}. Can not combine handlers with interfaces {nameof(ISequenceHandler)} or {nameof(IConcurrentHandler)} or without if any if these two interfaces. Please check handlers: {string.Join(", ", handlers)}");
        }

        public static MediatorException CreateForInvalidHandlers(params string[] errors)
        {
            var ex = new MediatorException($"Invalid handle configuration. For more details see Data property.");
            var i = 1;
            foreach (var error in errors)
            {
                ex.Data[FormatDataKey(i)] = error;
                i++;
            }

            return ex;
        }

        public static MediatorException TooManyPipelines(IMediatorAction action)
        {
            var ex = new MediatorException($"Too many pipelines met the condition for action execution. Please check your mediator configuration.");
            ex.Data["action"] = action;
            return ex;
        }
    }
}
