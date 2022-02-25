using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    public class ContextStackMiddleware : IMediatorMiddleware
    {
        private AsyncLocal<Stack<MediatorContext>> _asyncLocal = new();

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var stack = _asyncLocal.Value;
            var pushed = false;
            if (stack == null)
            {
                stack = _asyncLocal.Value = new();
            }
            else
            {
                pushed = true;
                stack.Push(context);
            }
            context.Features.Set(stack);
            try
            {
                await next(context);
            }
            finally
            {
                if (pushed)
                {
                    stack.Pop();
                }
            }
        }
    }
}
