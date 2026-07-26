using Demo.Shared;
using Pipaslot.Mediator.Middlewares;
using System.Net;
using Pipaslot.Mediator.Http;

namespace Demo.Server.MediatorMiddlewares;

public class ValidatorMiddleware : IMediatorMiddleware
{
    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        if (context.Action is IValidable validable)
        {
            var errors = validable.Validate();
            if (errors != null && errors.Any())
            {
                context.AddErrors(errors);
                context.SetResponseStatusCodeHint((int)HttpStatusCode.BadRequest);
               
                return;
            }
        }

        await next(context);
    }
}