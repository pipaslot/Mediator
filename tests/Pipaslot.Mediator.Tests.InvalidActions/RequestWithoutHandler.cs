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
        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            context.AddResult(new RequestWithoutHandler.ResultDto());

            return Task.CompletedTask;
            // If we provide result which should replace the handler execution result, we shouldnt go to the next middleware.
            // If we go, then the execution middleware will set MediatorContext.Status to NoHandlerFound leading to error produced. Then we have to reset the status bac to succeeded.
            //await next(context);
            //if(context.Status == ExecutionStatus.NoHandlerFound)
            //{
            //    context.Status = ExecutionStatus.Succeeded;
            //}
        }
    }
}
