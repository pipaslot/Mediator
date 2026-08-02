using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.Fixtures;

public class RemoveResultFromContextMiddleware : IMediatorMiddleware
{
    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        // The result will be attached to the fake context instead to the actual one
        var fakeContext = context.CopyEmpty();
        await next(fakeContext);
    }
}
