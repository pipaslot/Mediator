using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.Fixtures;

public class AddErrorAndEndMiddleware : IMediatorMiddleware
{
    public const string Error = "Fake error";

    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        context.AddError(Error);
        return Task.CompletedTask;
    }
}
