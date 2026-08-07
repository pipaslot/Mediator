using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator;

/// <summary>
/// Thrown when a handler or middleware set the ExecutionStatus to Failed without an original exception to rethrow.
/// </summary>
/// <remarks>
/// The fallback of <see cref="IMediator.DispatchUnhandled"/>/<see cref="IMediator.ExecuteUnhandled{TResult}"/> for a failure that
/// was only ever reported as an error message - a validation middleware calling <c>context.AddError(...)</c>, typically.
/// The messages are in <see cref="MediatorExecutionException.Response"/>. To let the original exception through instead,
/// record it with <see cref="Middlewares.MediatorContext.AddException"/>; to treat such a failure as data rather than as
/// an exception, call <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute{TResult}"/>.
/// <para>
/// On the HTTP client this is the normal outcome of every failure, not an edge case. An exception raised on the server is
/// translated at the server's own boundary and travels back as a failed response carrying error messages - never as an
/// exception - so the client pipeline has nothing original to rethrow and ends up here with the server's messages. The
/// same applies to a transport failure (unreachable server, unparsable response), which the client's execution middleware
/// also turns into an error message. Cancellation is the one exception that reaches the caller with its own type. A client
/// caller that needs to branch on the failure therefore has to read the messages, or call
/// <see cref="IMediator.Execute{TResult}"/>/<see cref="IMediator.Dispatch"/> and inspect the response instead.
/// </para>
/// </remarks>
public class MediatorUnhandledErrorException(string message, MediatorContext? context) : MediatorExecutionException(message, context)
{
    internal static MediatorUnhandledErrorException Create(MediatorContext context)
    {
        return Create($"'{GetErrors(context.Results)}'", context);
    }
    
    internal static MediatorUnhandledErrorException Create(string errors, MediatorContext context)
    {
        return new MediatorUnhandledErrorException(
            $"Handler or middlewares set the ExecutionStatus to {ExecutionStatus.Failed}. To prevent this exception, user methods Mediator.Dispatch or Mediator.Execute instead. Error messages: [{errors}]",
            context);
    }
}
