using Demo.Shared;
using Pipaslot.Mediator.Middlewares;

namespace Demo.Server.MediatorMiddlewares
{
    public class CommonMiddleware : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            //Attach custom message, validation result or any other data causing additional action on client besides to handler execution
            context.Results.Add(new CommonResult { Description = $"Middleware {nameof(CommonMiddleware)} is sending you a greeting as a custom result." });
            // Do something before handler execution
            await next(context);
            // Do something after handler execution
        }
    }
}
