using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares
{
    public class ExceptionLoggingMiddleware : IMediatorMiddleware
    {
        private readonly ILogger _logger;
        private readonly static JsonSerializerOptions _serializationOptions = new()
        {
            PropertyNamingPolicy = null
        };

        public ExceptionLoggingMiddleware(ILogger<ExceptionLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (OperationCanceledException oce)
            {
                var serializedData = Serialize(context.Action);
                _logger.LogWarning(oce, $"Action {context.ActionIdentifier} was canceled. Action content: {serializedData}");
                throw;
            }
            catch (Exception e)
            {
                var serializedData = Serialize(context.Action);
                _logger.LogError(e, @$"Exception occured during Mediator execution for action '{context.ActionIdentifier}' with message: '{e.Message}'. Action content: {serializedData}");
                throw;
            }
        }

        private string Serialize(object? obj)
        {
            if (obj == null)
            {
                return "NULL";
            }
            try
            {
                return JsonSerializer.Serialize(obj, _serializationOptions);
            }
            catch (Exception e)
            {
                return "Data serialization failed: " + e.Message;
            }
        }
    }
}
