using Pipaslot.Mediator;
using Pipaslot.Mediator.Http;
using Pipaslot.Mediator.Middlewares;
using System.Net;

namespace Demo.Server.MediatorMiddlewares;

/// <summary>
/// Translates a <see cref="ValidationException"/> raised by <see cref="ValidatorMiddleware"/> back into the same
/// per-field error messages and 400 status code the middleware used to add directly.
/// </summary>
public class ValidationExceptionHandler : IMediatorExceptionHandler<ValidationException>
{
    public Task Handle(ValidationException exception, IMediatorExceptionContext context)
    {
        context.Context.AddErrors(exception.Errors);
        context.Context.SetResponseStatusCodeHint((int)HttpStatusCode.BadRequest);
        context.SetHandledWithoutMessage();
        return Task.CompletedTask;
    }
}
