using Pipaslot.Mediator;
using Pipaslot.Mediator.Middlewares;

namespace Demo.Server.MediatorMiddlewares
{
    public class MediatorCallStackLoggerMiddleware : IMediatorMiddleware
    {
        private readonly ILogger<MediatorCallStackLoggerMiddleware> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        public MediatorCallStackLoggerMiddleware(ILogger<MediatorCallStackLoggerMiddleware> logger, IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var isRequest = _contextAccessor.HttpContext != null;
            var stack = CallStackHelper.GetHandlerExecutionStack();

            var calls = stack
                .Select(s => s.AssemblyQualifiedName)
                .ToList();
            var source = isRequest ? "HTTP REQUEST" : "SERVER";
            var msg = $"Action {context.ActionIdentifier} executed by {source} from handlers: {string.Join(" -> ", calls)}";
            _logger.LogInformation(msg);

            await next(context);
        }
    }
}
