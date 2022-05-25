using Pipaslot.Mediator.Abstractions;
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
            return new MediatorException($"Multiple handlers were registered for the same action. Remove one from defined type: {string.Join(" OR ", handlers)}");
        }

        public static MediatorException CreateForNoActionRegistered()
        {
            return new MediatorException($"No action marker assembly was registered. Use {nameof(PipelineConfigurator.AddActionsFromAssembly)} during pipeline setup");
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

        public static MediatorException CreateForNoActionType(Type type)
        {
            return new MediatorException($"Type {type} does not implements {nameof(IMediatorAction)} interface");
        }

        public static MediatorException CreateForNoHandlerType(Type type)
        {
            return new MediatorException($"Type {type} does not implements {nameof(IMediatorHandler<IMediatorAction>)} interface");
        }

    }
}
