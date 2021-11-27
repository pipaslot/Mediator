﻿using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Server
{
    public class MediatorExceptionLoggingMiddleware : IMediatorMiddleware
    {
        private readonly ILogger _logger;
        private readonly static JsonSerializerOptions _serializationOptions = new()
        {
            PropertyNamingPolicy = null
        };

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
                var serializedData = Serialize(action);
                _logger.LogError(e, @$"Exception occured during Mediator execution for action '{action?.GetType()}' with message: '{e.Message}'. Action content: {serializedData}");
                throw;
            }
        }

        private string Serialize(object? obj)
        {
            if(obj == null)
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
