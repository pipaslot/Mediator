﻿using Pipaslot.Mediator.Middlewares;
using Demo.Shared;
using System.Net;

namespace Demo.Server.MediatorMiddlewares
{
    public class ValidatorMiddleware : IMediatorMiddleware
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ValidatorMiddleware(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            if(action is IValidable validable)
            {
                var errors = validable.Validate();
                if (errors != null && errors.Any())
                {
                    context.ErrorMessages.AddRange(errors);
                    // Optional:
                    // Notify the client via response status code to imporove logging and debugging experience
                    var httpContext = _httpContextAccessor.HttpContext;
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
}
