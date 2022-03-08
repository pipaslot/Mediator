using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions
{
    public class AddErrorAndEndMiddleware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddError("Fake error");
            return Task.CompletedTask;
        }
    }
}
