using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Pipaslot.Mediator;
using Pipaslot.Mediator.Middlewares;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Server.MediatorMiddlewares
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

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            var isRequest = _contextAccessor.HttpContext != null;
            var stack = CallStackHelper.GetHandlerExecutionStack();

            var calls = stack
                .Select(s => s.AssemblyQualifiedName)
                .ToList();
            var source = isRequest ? "HTTP REQUEST" : "SERVER";
            var msg = $"Action {action.GetType().AssemblyQualifiedName} executed by {source} from handlers: {string.Join(" -> ", calls)}";
            _logger.LogInformation(msg);

            await next(context);
        }
    }
}
