using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Authorization
{
    /// <summary>
    /// Evaluate authorization policies for actions and their handlers.
    /// </summary>
    public class AuthorizationMiddleware : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            await PolicyResolver.CheckPolicies(context.Services, context.Action, context.GetHandlers(), context.CancellationToken).ConfigureAwait(false);
            await next(context).ConfigureAwait(false);
        }
    }
}
