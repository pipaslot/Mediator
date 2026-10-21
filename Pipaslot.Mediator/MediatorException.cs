using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using System;

namespace Pipaslot.Mediator;

/// <summary>
/// Misconfiguration or misuse of the mediator: invalid registration, conflicting handlers or pipelines, or an action
/// called where it is not allowed. Also the base type of the execution failures, see <see cref="MediatorExecutionException"/>.
/// </summary>
/// <remarks>
/// Most of these are thrown while the services are being configured, or by the startup handler check, and point to a
/// bug to fix rather than a failure to handle. Thrown while an action executes, it is treated like any other exception:
/// <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute{TResult}"/> log it and report the message of a registered
/// <see cref="IMediatorExceptionHandler{TException}"/>, or the generic one,
/// the <c>*Unhandled</c> entry points let it through. The static factories document when each case occurs.
/// </remarks>
public class MediatorException(string message) : Exception(message)
{
    internal static string FormatDataKey(int number)
    {
        return $"Error:{number}";
    }

    /// <summary>
    /// Thrown on execution when an action has more than one handler and none of them is marked with
    /// <see cref="ISequenceHandler"/> or <see cref="IConcurrentHandler"/>. The startup handler check reports the same
    /// case through <see cref="CreateForInvalidHandlers"/>.
    /// </summary>
    public static MediatorException CreateForDuplicateHandlers<THandler>(Type subject, THandler[] handlers)
    {
        return new MediatorException(
            $"Multiple handlers were registered for one action type: {subject}. Only one handler was expected. Remove one from defined type: {string.Join(" OR ", handlers)}");
    }

    /// <summary>
    /// Thrown on execution when the handlers of one action mix <see cref="ISequenceHandler"/>, <see cref="IConcurrentHandler"/>
    /// and unmarked handlers - all handlers of an action have to use the same mode. The startup handler check reports the
    /// same case through <see cref="CreateForInvalidHandlers"/>.
    /// </summary>
    public static MediatorException CreateForCanNotCombineHandlers<THandler>(Type subject, THandler[] handlers)
    {
        return new MediatorException(
            $"Multiple handlers were registered for one action type: {subject}. Can not combine handlers with interfaces {nameof(ISequenceHandler)} or {nameof(IConcurrentHandler)} or without if any if these two interfaces. Please check handlers: {string.Join(", ", handlers)}");
    }

    /// <summary>
    /// Thrown by <see cref="Services.IHandlerExistenceChecker.Verify"/> with every problem it found at once - missing,
    /// duplicate or incompatible handlers, or missing authorization policies. Each message is also stored in
    /// <see cref="Exception.Data"/> under the keys <c>Error:1</c>, <c>Error:2</c>, ...
    /// </summary>
    public static MediatorException CreateForInvalidHandlers(params string[] errors)
    {
        var joined = "[" + string.Join(", ", errors) + "]";
        var ex = new MediatorException("Invalid handle configuration. For more details see Data property. " + joined);
        var i = 1;
        foreach (var error in errors)
        {
            ex.Data[FormatDataKey(i)] = error;
            i++;
        }

        return ex;
    }

    /// <summary>
    /// Thrown by <c>AddActions</c> for a type that does not implement <see cref="IMediatorAction"/>.
    /// </summary>
    public static MediatorException CreateForNoActionType(Type type)
    {
        return new MediatorException($"Type {type} does not implements {nameof(IMediatorAction)} interface");
    }

    /// <summary>
    /// Thrown by <c>AddHandlers</c> for a type that implements neither <see cref="IMediatorHandler{TAction}"/> nor
    /// <see cref="IMediatorHandler{TAction, TResult}"/>.
    /// </summary>
    public static MediatorException CreateForNoHandlerType(Type type)
    {
        return new MediatorException($"Type {type} does not implements {nameof(IMediatorHandler<IMediatorAction>)} interface");
    }

    /// <summary>
    /// Thrown on execution when the conditions of more than one pipeline registered via <c>AddPipeline</c> match
    /// <paramref name="action"/> - an action may run through one pipeline only. The action is stored in
    /// <see cref="Exception.Data"/> under the key <c>action</c>.
    /// </summary>
    public static MediatorException TooManyPipelines(IMediatorAction action)
    {
        var ex = new MediatorException("Too many pipelines met the condition for action execution. Please check your mediator configuration.");
        ex.Data["action"] = action;
        return ex;
    }

    /// <summary>
    /// Thrown during authorization when a handler's <see cref="Authorization.IHandlerAuthorization{TAction}.Authorize"/> or
    /// <see cref="Authorization.IHandlerAuthorizationAsync{TAction}.AuthorizeAsync"/> returns null instead of a policy.
    /// <paramref name="what"/> is the handler type name.
    /// </summary>
    public static MediatorException NullInsteadOfPolicy(string what)
    {
        return new MediatorException($"Object {what} returned NULL instead of policy");
    }

    /// <summary>
    /// Thrown by <see cref="Middlewares.DirectCallProtectionMiddleware"/> when a protected action is called as the
    /// outermost mediator call instead of as a nested call from another handler or middleware.
    /// </summary>
    public static MediatorException CreateForForbidenDirectCall()
    {
        return new MediatorException(
            "Executed action can not be executed directly. It is expected to be used as nested call only (inside another handler or middleware).");
    }

    /// <summary>
    /// Thrown by <c>AddHandlers</c> when the lifetime passed explicitly conflicts with the one required by
    /// <see cref="ISingleton"/>/<see cref="IScoped"/> on the handler, or when the same handler type is registered again
    /// with a different lifetime.
    /// </summary>
    public static MediatorException CreateForWrongHandlerServiceLifetime(Type handlerType, ServiceLifetime expected, ServiceLifetime actual)
    {
        return new MediatorException(
            $"Handler type {handlerType} can be registered only with '{expected}' ServiceLifetime but was registered as '{actual}'.");
    }

    /// <summary>
    /// Thrown by <c>AddExceptionHandler</c>/<c>AddExceptionHandlers</c> for a type that does not implement
    /// <see cref="IMediatorExceptionHandler{TException}"/>.
    /// </summary>
    public static MediatorException CreateForNoExceptionHandlerType(Type type)
    {
        return new MediatorException($"Type {type} does not implement {nameof(IMediatorExceptionHandler<Exception>)} interface");
    }

    internal static MediatorException CreateForContextWithoutServices(Type serviceType)
    {
        return new MediatorException(
            $"Resolving {serviceType} failed because this {nameof(MediatorContext)} was created by {nameof(MediatorContext)}.{nameof(MediatorContext.Create)} " +
            $"without an {nameof(IServiceProvider)}. Pass one in to exercise code paths resolving services, for example {nameof(MediatorContext.GetHandlers)}().");
    }

    internal static MediatorException CreateForContextWithoutMediator(IMediatorAction action)
    {
        var ex = new MediatorException(
            $"Nested call of action {action.GetType()} failed because this {nameof(MediatorContext)} was created by {nameof(MediatorContext)}.{nameof(MediatorContext.Create)} " +
            $"without an {nameof(IMediator)}. Pass one in - typically a test double - to exercise code paths making nested calls.");
        ex.Data["action"] = action;
        return ex;
    }

    /// <summary>
    /// Thrown by <c>AddExceptionHandler</c>/<c>AddExceptionHandlers</c> when a handler for the same exception type is
    /// already registered. Replace an existing handler on the <c>IServiceCollection</c> instead, as the message describes.
    /// </summary>
    public static MediatorException CreateForDuplicateExceptionHandler(Type exceptionType)
    {
        return new MediatorException(
            $"An exception handler for type {exceptionType} is already registered. Only one handler can be registered per exception type through the configurator. " +
            $"To replace it (e.g. in tests), call services.AddTransient<IMediatorExceptionHandler<{exceptionType.Name}>, YourHandler>() " +
            "on the IServiceCollection instead of calling AddExceptionHandler/AddExceptionHandlers again for the same type.");
    }
}