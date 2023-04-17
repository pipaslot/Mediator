using Pipaslot.Mediator.Middlewares;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using static Pipaslot.Mediator.Tests.InvalidActions.BlockedRequest;

namespace Pipaslot.Mediator.Tests.InvalidActions
{
    public class BlockedRequest : IRequest<ResultDto>
    {
        public class ResultDto
        {

        }
    }
    /// <summary>
    /// Simulate that the expected DTO was attached event if the action does not have handler
    /// </summary>
    public class BlockRequestMilldeware : IMediatorMiddleware
    {
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.Status = ExecutionStatus.Failed;
            // Do not run next delegate
            return Task.CompletedTask;
        }
    }
}
