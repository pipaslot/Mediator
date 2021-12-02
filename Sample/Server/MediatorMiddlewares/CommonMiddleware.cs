using Pipaslot.Mediator.Middlewares;
using Sample.Shared;

namespace Sample.Server.MediatorMiddlewares
{
    public class CommonMiddleware : IMediatorMiddleware
    {
        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            //Attach custom message, validation result or any other data causing additional action on client besides to handler execution
            context.Results.Add(new CommonResult { Description = $"Middleware {nameof(CommonMiddleware)} is sending you a greeting as a custom result." });
            // Do something before handler execution
            await next(context);
            // Do something after handler execution
        }
    }
}
