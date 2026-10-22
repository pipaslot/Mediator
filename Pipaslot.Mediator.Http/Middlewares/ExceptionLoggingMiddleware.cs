using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares;

/// <summary>
/// Register exception logging middleware writting all error into <see cref="ILogger"/>
/// </summary>
/// <remarks>
/// Logs the exception and rethrows it, so it only observes failures and does not change the outcome. Cancellation is
/// logged as a warning, other exceptions as errors; the serialized action is added at debug level, so it may contain
/// sensitive data. Register with <see cref="MiddlewareRegistratorExtensions.UseExceptionLogging"/>.
/// </remarks>
public class ExceptionLoggingMiddleware(ILogger<ExceptionLoggingMiddleware> logger) : IMediatorMiddleware
{
    private readonly ILogger _logger = logger;
    private static readonly JsonSerializerOptions _serializationOptions = new() { PropertyNamingPolicy = null };

    /// <inheritdoc/>
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