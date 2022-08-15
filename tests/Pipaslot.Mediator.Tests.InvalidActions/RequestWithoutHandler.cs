using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;
using static Pipaslot.Mediator.Tests.InvalidActions.RequestWithoutHandler;

namespace Pipaslot.Mediator.Tests.InvalidActions
{

    public class RequestWithoutHandler : IRequest<ResultDto>
    {
        public class ResultDto
        {

        }
    }
    /// <summary>
    /// Simulate that the expected DTO was attached event if the action does not have handler
    /// </summary>
    public class RequestWithoutHandlerAttacheResultMilldeware : IMediatorMiddleware
    {
        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddResult(new RequestWithoutHandler.ResultDto());
            await next(context);
        }
    }
}
