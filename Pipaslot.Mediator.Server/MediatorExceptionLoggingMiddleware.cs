using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Server
{
    public class MediatorExceptionLoggingMiddleware : IMediatorMiddleware
    {
        private ILogger<MediatorExceptionLoggingMiddleware> _logger;

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

                // We need to write the error in context or re-throw the exception to let mediator know that processing failed
                context.ErrorMessages.Add(e.Message);
            }
        }
    }
}
