using Pipaslot.Mediator;
using Pipaslot.Mediator.Middlewares;

namespace Demo.Server.MediatorMiddlewares
{
    public class CallStackLoggerMiddleware : IMediatorMiddleware
    {
        private readonly ILogger<CallStackLoggerMiddleware> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMediatorContextAccessor _mediatorContextAccessor;

        public CallStackLoggerMiddleware(ILogger<CallStackLoggerMiddleware> logger, IHttpContextAccessor contextAccessor, IMediatorContextAccessor mediatorContextAccessor)
        {
            _logger = logger;
            _contextAccessor = contextAccessor;
            _mediatorContextAccessor = mediatorContextAccessor;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var isRequest = _contextAccessor.HttpContext != null;

            var stack = _mediatorContextAccessor.ContextStack;
            if (stack.Count > 0)
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
