using System;
using System.Threading.Tasks;

namespace Pipaslot.Mediator;

/// <summary>
/// Ready-to-use exception handler reporting a cancelled operation as an expected failure instead of the generic
/// "unexpected error" message, and downgrading its log entry from Error to Warning. Covers <see cref="TaskCanceledException"/>
/// as well, since it derives from <see cref="OperationCanceledException"/>.
/// Opt-in - register it via <see cref="Configuration.IMediatorConfigurator.AddExceptionHandler{THandler}"/>, because whether
/// a cancelled action is routine or worth investigating depends on the application.
/// Derive from this class and override <see cref="GetMessage"/> to localize the message.
/// </summary>
public class OperationCanceledExceptionHandler : IMediatorExceptionHandler<OperationCanceledException>
{
    /// <summary>
    /// Message reported to <see cref="IMediator.Dispatch"/>/<see cref="IMediator.Execute{TResult}"/> callers for a cancelled operation.
    /// </summary>
    public const string Message = "The operation was cancelled.";

    public virtual Task Handle(OperationCanceledException exception, IMediatorExceptionContext context)
    {
        context.SetHandled(GetMessage(exception));
        return Task.CompletedTask;
    }

    /// <summary>
    /// The message reported to the client for <paramref name="exception"/>. Override to localize.
    /// </summary>
    protected virtual string GetMessage(OperationCanceledException exception) => Message;
}
