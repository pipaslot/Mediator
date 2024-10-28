using Pipaslot.Mediator;
using Pipaslot.Mediator.Middlewares;

namespace Demo.Server.MediatorMiddlewares;

public class CallStackLoggerMiddleware(
    ILogger<CallStackLoggerMiddleware> logger,
    IHttpContextAccessor contextAccessor,
    IMediatorContextAccessor mediatorContextAccessor)
    : IMediatorMiddleware
{
    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        var isRequest = contextAccessor.HttpContext != null;

        var stack = mediatorContextAccessor.ContextStack;
        if (stack.Count > 0)
        {
            var calls = stack
                .Select(s => s.Action.GetType().AssemblyQualifiedName)
                .ToList();
            var source = isRequest ? "HTTP REQUEST" : "SERVER";
            var msg = $"Action {context.ActionIdentifier} executed by {source} from handlers: {string.Join(" -> ", calls)}";
            logger.LogInformation(msg);
        }

        await next(context);
    }
}