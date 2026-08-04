using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.InvalidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E.Fixtures;

/// <summary>
/// Simulate that the expected DTO was attached event if the action does not have handler
/// </summary>
public class RequestWithoutHandlerAttacheResultMiddleware : IMediatorMiddleware
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
