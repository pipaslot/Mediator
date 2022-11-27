using Pipaslot.Mediator.Middlewares;
using System.Linq;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    public class AuthorizationMiddleware : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            await PolicyResolver.CheckPolicies(context.Services, context.Action, context.GetHandlers(), context.CancellationToken);
            await next(context);
        }
    }
}
