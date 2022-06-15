using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests
{
    /// <summary>
    /// Just catch all eception from handlers. Tests with this middleware proves that errors from handlers are processed correctly
    /// </summary>
    public class ExceptionConsumingMiddleware : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            try
            {
                await next(context);
            }
            catch
            {
                // Catch all silently
            }
        }
    }
}
