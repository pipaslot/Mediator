using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Middlewares;

namespace Demo.Server.MediatorMiddlewares;

public class CustomLoggingMiddleware(ILogger<CustomLoggingMiddleware> logger) : IMediatorMiddleware
{
    private readonly ILogger _logger = logger;

    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        // Catch all exceptions and provide only unified message
        try
        {
            await next(context);
        }
        catch (Exception c) when (c is TaskCanceledException || c is OperationCanceledException)
        {
            context.Status = ExecutionStatus.Failed;
            // No error message is needed
        }
        catch (AuthorizationException ae)
        {
            context.AddError(ae.Message, "CustomLogging of action: " + context.Action.GetActionFriendlyName());
            _logger.LogWarning(ae, $"Unauthorized invocation of action '{context.ActionIdentifier}'");
        }
        catch (Exception e)
        {
            context.AddError("Operation failed, please contact administrator", "CustomLogging of action: " + context.Action.GetActionFriendlyName());
            _logger.LogError(e, @$"Exception occured during Mediator execution for action '{context.ActionIdentifier}' with message: '{e.Message}'.");
        }
    }
}