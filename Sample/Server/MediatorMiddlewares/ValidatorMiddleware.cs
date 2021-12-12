using Pipaslot.Mediator.Middlewares;
using Sample.Shared;

namespace Sample.Server.MediatorMiddlewares
{
    public class ValidatorMiddleware : IMediatorMiddleware
    {
        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            if(action is IValidable validable)
            {
                var errors = validable.Validate();
                if (errors != null && errors.Any())
                {
                    context.ErrorMessages.AddRange(errors);
                    return;
                }
            }
            await next(context);
        }
    }
}
