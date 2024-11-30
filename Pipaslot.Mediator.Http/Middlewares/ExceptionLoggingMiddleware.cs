using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares;

/// <summary>
/// Register exception logging middleware writting all error into <see cref="ILogger"/>
/// </summary>
public class ExceptionLoggingMiddleware(ILogger<ExceptionLoggingMiddleware> logger) : IMediatorMiddleware
{
    private readonly ILogger _logger = logger;
    private static readonly JsonSerializerOptions _serializationOptions = new() { PropertyNamingPolicy = null };

    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception ce) when (ce is TaskCanceledException or OperationCanceledException)
        {
            _logger.LogWarning(ce, $"Action {context.ActionIdentifier} was canceled.");
            var serializedData = Serialize(context.Action);
            _logger.LogDebug($"The failed Mediator action'{context.ActionIdentifier}' was executed with content: {serializedData}");
            throw;
        }
        catch (Exception e)
        {
            _logger.LogError(e, @$"Exception occured during Mediator execution for action '{context.ActionIdentifier}' with message: '{e.Message}'.");
            var serializedData = Serialize(context.Action);
            _logger.LogDebug($"The failed Mediator action'{context.ActionIdentifier}' was executed with content: {serializedData}");
            throw;
        }
    }

    private static string Serialize(object? obj)
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