using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Server.MediatorMiddlewares;

/// <summary>
/// Reports an <see cref="AuthorizationException"/> to the client with its own message instead of the generic
/// fallback. Downgrades the boundary's log entry to Information, since an authorization failure is routine here
/// rather than a Warning-worthy event.
/// </summary>
public class AuthorizationExceptionHandler : IMediatorExceptionHandler<AuthorizationException>
{
    public Task Handle(AuthorizationException exception, IMediatorExceptionContext context)
    {
        context.SetHandled(exception.Message);
        context.SetLogLevel(LogLevel.Information);
        return Task.CompletedTask;
    }
}
