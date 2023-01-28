﻿using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Middlewares;
using System.Text.Json;

namespace Demo.Server.MediatorMiddlewares
{
    public class CustomLoggingMiddleware : IMediatorMiddleware
    {
        private readonly ILogger _logger;
        private readonly static JsonSerializerOptions _serializationOptions = new()
        {
            PropertyNamingPolicy = null
        };

        public CustomLoggingMiddleware(ILogger<CustomLoggingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            // Prevent ActionErrors logged from exceptions in execution middleware
            // Catch all exceptions and provide only unified message
            try
            {
                context.IgnoreActionErrors = true;
                await next(context);
            }
            catch (Exception c) when (c is TaskCanceledException || c is OperationCanceledException)
            {
                context.Status = ExecutionStatus.Failed;
                // No error message is needed
            }
            catch (AuthorizationException ae)
            {
                context.AddError(ae.Message, "CustomLogging of action: " + context.Action.GetActionFriendlyName());
                _logger.LogWarning(ae, $"Unauthorized invocation of action '{context.ActionIdentifier}'");
            }
            catch (Exception e)
            {
                context.AddError("Operation failed, please contact administrator", "CustomLogging of action: " + context.Action.GetActionFriendlyName());
                _logger.LogError(e, @$"Exception occured during Mediator execution for action '{context.ActionIdentifier}' with message: '{e.Message}'.");
            }
        }
    }
}
