using Pipaslot.Mediator.Middlewares;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Server
{
    public class CommonMiddleware : IMediatorMiddleware
    {
        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            // Do something before handler execution
            await next(context);
            // Do something after handler execution
        }
    }
}
