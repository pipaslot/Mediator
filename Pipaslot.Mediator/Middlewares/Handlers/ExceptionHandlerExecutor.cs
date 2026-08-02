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
    /// Resolves and invokes the typed handler. Returns false when the handler could not be resolved from DI or threw
    /// - both degrade to the same "no match" outcome so the caller can fall back to the default translation. Returns
    /// true when the handler ran to completion; whether it actually called <see cref="IMediatorExceptionContext.SetHandled"/>/
    /// <see cref="IMediatorExceptionContext.SetHandledWithoutMessage"/> is then read back off <paramref name="context"/> by the caller.
    /// </summary>
    internal abstract Task<bool> Handle(Exception exception, IServiceProvider services, IMediatorExceptionContext context);
}

internal sealed class ExceptionHandlerExecutor<TException> : ExceptionHandlerExecutor
    where TException : Exception
{
    internal override async Task<bool> Handle(Exception exception, IServiceProvider services, IMediatorExceptionContext context)
    {
        // GetService (not GetRequiredService): the resolved routing entry must tolerate a handler removed from DI
        // (e.g. via RemoveAll in tests) and degrade to the "not handled" fallback instead of throwing.
        var handler = services.GetService<IMediatorExceptionHandler<TException>>();
        if (handler is null)
        {
            return false;
        }

        try
        {
            await handler.Handle((TException)exception, context).ConfigureAwait(false);
            return true;
        }
        catch
        {
            // A throwing handler must never mask the original exception - it degrades to "not handled" here;
            // the caller is responsible for logging this as a secondary failure.
            return false;
        }
    }
}
