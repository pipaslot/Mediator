using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Server
{
    public class MediatorExceptionLoggingMiddleware : IMediatorMiddleware
    {
        private readonly ILogger<MediatorExceptionLoggingMiddleware> _logger;

        public MediatorExceptionLoggingMiddleware(ILogger<MediatorExceptionLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            try
            {
                await next(context);
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Exception occured during Mediator execution: " + e.Message);
                throw;
            }
        }
    }
}
