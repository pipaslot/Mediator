using Demo.Shared;
using Pipaslot.Mediator.Middlewares;

namespace Demo.Server.MediatorMiddlewares;

public class ValidatorMiddleware : IMediatorMiddleware
{
    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        if (context.Action is IValidable validable)
        {
            var errors = validable.Validate();
            if (errors.Any())
            {
                context.AddException(new ValidationException(errors));
                return;
            }
        }

        await next(context);
    }
}
