using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System;

namespace Pipaslot.Mediator;

/// <summary>
/// General mediator exception
/// </summary>
public class MediatorException : Exception
{
    internal static string FormatDataKey(int number)
    {
        return $"Error:{number}";
    }

    public MediatorException(string message) : base(message)
    {
    }

    public static MediatorException CreateForDuplicateHandlers(Type subject, object[] handlers)
    {
        return new MediatorException(
            $"Multiple handlers were registered for one action type: {subject}. Only one handler was expected. Remove one from defined type: {string.Join(" OR ", handlers)}");
    }

    public static MediatorException CreateForCanNotCombineHandlers(Type subject, object[] handlers)
    {
        return new MediatorException(
            $"Multiple handlers were registered for one action type: {subject}. Can not combine handlers with interfaces {nameof(ISequenceHandler)} or {nameof(IConcurrentHandler)} or without if any if these two interfaces. Please check handlers: {string.Join(", ", handlers)}");
    }

    public static MediatorException CreateForInvalidHandlers(params string[] errors)
    {
        var joined = "[" + string.Join(", ", errors) + "]";
        var ex = new MediatorException($"Invalid handle configuration. For more details see Data property. " + joined);
        var i = 1;
        foreach (var error in errors)
        {
            ex.Data[FormatDataKey(i)] = error;
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

    public static MediatorException TooManyPipelines(IMediatorAction action)
    {
        var ex = new MediatorException($"Too many pipelines met the condition for action execution. Please check your mediator configuration.");
        ex.Data["action"] = action;
        return ex;
    }

    public static MediatorException NullInsteadOfPolicy(string what)
    {
        return new MediatorException($"Object {what} returned NULL instead of policy");
    }

    public static MediatorException CreateForForbidenDirectCall()
    {
        return new MediatorException(
            $"Executed action can not be executed directly. It is expected to be used as nested call only (inside another handler or middleware).");
    }

    public static MediatorException CreateForWrongHandlerServiceLifetime(Type handlerType, ServiceLifetime expected, ServiceLifetime actual)
    {
        return new MediatorException(
            $"Handler type {handlerType} can be registered only with '{expected}' ServiceLifetime but was registered as '{actual}'.");
    }
}