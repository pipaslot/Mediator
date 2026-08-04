using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.Fixtures;

public class BlockedRequest : IRequest<BlockedRequest.ResultDto>
{
    public class ResultDto;
}

public class BlockRequestMiddleware : IMediatorMiddleware
{
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        context.Status = ExecutionStatus.Failed;
        // Do not run next delegate
        return Task.CompletedTask;
    }
}
