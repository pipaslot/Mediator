using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
/// Ready-to-use exception handler restoring the pre-"safe by default" behavior (see <see cref="IMediatorExceptionContext"/>):
/// reports every otherwise-untranslated exception's own <see cref="Exception.Message"/> verbatim to
/// <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute{TResult}"/> callers, instead of the generic message the
/// boundary uses by default.
/// <para>
/// Registered for <see cref="Exception"/>, so it also becomes the catch-all for every exception type that has no more
/// specific handler registered, the same way a hand-written <c>catch (Exception e) { context.AddError(e.Message); }</c>
/// middleware used to. Registration order does not matter for this: exception handler resolution always picks the
/// most specific registered type for the thrown exception, so a handler registered for a narrower type still wins over
/// this one regardless of which was added first - see <see cref="Configuration.IMediatorConfigurator.AddExceptionHandler{THandler}"/>.
/// </para>
/// <para>
/// Registering this handler re-opens the exact leak the safe-by-default boundary exists to close: <c>Exception.Message</c>
/// routinely contains connection strings, file paths, SQL fragments, or entity identifiers, and in Client-Server usage
/// that message is serialized straight to a browser. Use it only as a temporary bridge while migrating callers that
/// depended on the old behavior, and replace it with typed <see cref="IMediatorExceptionHandler{TException}"/>
/// registrations for the specific exception types you actually want to translate as soon as you can.
/// </para>
/// Opt-in - register it explicitly via <see cref="Configuration.IMediatorConfigurator.AddExceptionHandler{THandler}"/>.
/// </summary>
public class LegacyExceptionMessageHandler : IMediatorExceptionHandler<Exception>
{
    public Task Handle(Exception exception, IMediatorExceptionContext context)
    {
        context.SetHandled(exception.Message);
        return Task.CompletedTask;
    }
}
