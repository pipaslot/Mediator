using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares.Handlers;

/// <summary>
/// Abstraction for generic exception handler executors to avoid reflection on the hot path.
/// </summary>
internal abstract class ExceptionHandlerExecutor
{
    internal abstract Task<ExceptionHandlerResult> Handle(Exception exception, IServiceProvider services, CancellationToken cancellationToken);
}

internal sealed class ExceptionHandlerExecutor<TException> : ExceptionHandlerExecutor
    where TException : Exception
{
    internal override async Task<ExceptionHandlerResult> Handle(Exception exception, IServiceProvider services, CancellationToken cancellationToken)
    {
        // GetService (not GetRequiredService): the resolved routing entry must tolerate a handler removed from DI
        // (e.g. via RemoveAll in tests) and degrade to the "not handled" fallback instead of throwing.
        var handler = services.GetService<IMediatorExceptionHandler<TException>>();
        if (handler is null)
        {
            return ExceptionHandlerResult.NotHandled;
        }

        try
        {
            var message = await handler.Handle((TException)exception, cancellationToken).ConfigureAwait(false);
            return ExceptionHandlerResult.Handled(message);
        }
        catch
        {
            // A throwing handler must never mask the original exception - it degrades to "not handled" here;
            // the caller is responsible for logging this as a secondary failure.
            return ExceptionHandlerResult.NotHandled;
        }
    }
}
