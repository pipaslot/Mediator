using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares.Handlers;

/// <summary>
/// Abstraction for generic exception handler executors to avoid reflection on the hot path.
/// </summary>
internal abstract class ExceptionHandlerExecutor
{
    /// <summary>
    /// Resolves and invokes the typed handler. Returns null when the handler could not be resolved from DI, or when
    /// it ran to completion without throwing - whether it actually called <see cref="IMediatorExceptionContext.SetHandled"/>/
    /// <see cref="IMediatorExceptionContext.SetHandledWithoutMessage"/> is then read back off <paramref name="context"/>
    /// by the caller. Returns the handler's own exception when it threw, so the caller can log that as a fault
    /// distinguishable from "no handler registered" or "handler declined".
    /// </summary>
    internal abstract Task<Exception?> Handle(Exception exception, IServiceProvider services, IMediatorExceptionContext context);
}

internal sealed class ExceptionHandlerExecutor<TException> : ExceptionHandlerExecutor
    where TException : Exception
{
    internal override async Task<Exception?> Handle(Exception exception, IServiceProvider services, IMediatorExceptionContext context)
    {
        // GetService (not GetRequiredService): the resolved routing entry must tolerate a handler removed from DI
        // (e.g. via RemoveAll in tests) and degrade to the "not handled" fallback instead of throwing.
        var handler = services.GetService<IMediatorExceptionHandler<TException>>();
        if (handler is null)
        {
            return null;
        }

        try
        {
            await handler.Handle((TException)exception, context).ConfigureAwait(false);
            return null;
        }
        catch (Exception handlerException)
        {
            // The handler's own exception must never mask the original exception - the caller falls back to
            // "not handled" and logs this exception separately as a distinct, secondary fault.
            return handlerException;
        }
    }
}
