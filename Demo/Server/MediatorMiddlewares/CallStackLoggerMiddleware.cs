using Pipaslot.Mediator.Middlewares;

namespace Demo.Server.MediatorMiddlewares
{
    public class CallStackLoggerMiddleware : IMediatorMiddleware
    {
        private readonly ILogger<CallStackLoggerMiddleware> _logger;
        private readonly IHttpContextAccessor _contextAccessor;

        public CallStackLoggerMiddleware(ILogger<CallStackLoggerMiddleware> logger, IHttpContextAccessor contextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var isRequest = _contextAccessor.HttpContext != null;

            var stack = context.Features.Get<Stack<MediatorContext>?>();
            if (stack != null && stack.Count > 0)
            {
                var calls = stack
                    .Select(s => s.Action.GetType().AssemblyQualifiedName)
                    .ToList();
                var source = isRequest ? "HTTP REQUEST" : "SERVER";
                var msg = $"Action {context.ActionIdentifier} executed by {source} from handlers: {string.Join(" -> ", calls)}";
                _logger.LogInformation(msg);
            }
            await next(context);
        }
    }
}
