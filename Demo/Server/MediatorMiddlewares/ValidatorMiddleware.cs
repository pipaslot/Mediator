using Demo.Shared;
using Pipaslot.Mediator.Middlewares;
using System.Net;

namespace Demo.Server.MediatorMiddlewares;

public class ValidatorMiddleware(IHttpContextAccessor httpContextAccessor) : IMediatorMiddleware
{
    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        if (context.Action is IValidable validable)
        {
            var errors = validable.Validate();
            if (errors != null && errors.Any())
            {
                context.AddErrors(errors);
                // Optional:
                // Notify the client via response status code to imporove logging and debugging experience
                var httpContext = httpContextAccessor.HttpContext;
                if (httpContext != null)
                {
                    httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }

                return;
            }
        }

        await next(context);
    }
}