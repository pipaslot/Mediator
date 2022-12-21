using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares
{
    public class HttpClientExecutionMiddleware : IExecutionMiddleware, IMediatorUrlFormatter
    {
        private readonly HttpClient _httpClient;
        private readonly ClientMediatorOptions _options;
        private readonly IContractSerializer _serializer;
        private readonly ILogger _logger;

        public HttpClientExecutionMiddleware(HttpClient httpClient, ClientMediatorOptions options, IContractSerializer serializer, ILogger<HttpClientExecutionMiddleware> logger)
        {
            _httpClient = httpClient;
            _options = options;
            _serializer = serializer;
            _logger = logger;
        }

        public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            var response = await SendRequest<object>(context);
            context.Append(response);
        }

        public string FormatHttpGet(IMediatorAction action)
        {
            var serialized = _serializer.SerializeRequest(action);
            var decoded = WebUtility.UrlDecode(serialized);
            return $"{_options.Endpoint}?type={action.GetType()}&{MediatorConstants.ActionQueryParamName}={decoded}";
        }

        protected virtual async Task<IMediatorResponse<TResult>> SendRequest<TResult>(MediatorContext context)
        {
            HttpResponseMessage response;
            try
            {
                var url = _options.Endpoint + $"?type={context.ActionIdentifier}";
                var json = _serializer.SerializeRequest(context.Action);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                response = await _httpClient.PostAsync(url, content, context.CancellationToken);
                // We do not check for successfull status code.
                // It is completelly up to server configuration what status code will be sent when action processing failed on server.
                // We just expect that server will return JSON in Mediator response format
                // Use ProcessParsingError for handling custom server responses and status codes 
            }
            catch (Exception ce) when (ce is OperationCanceledException || ce is TaskCanceledException)
            {
                context.Status = ExecutionStatus.Failed;
                return new MediatorResponse<TResult>("Operation was canceled", context.Action);
            }
            catch (Exception e)
            {
                context.Status = ExecutionStatus.Failed;
                return await ProcessRuntimeError<TResult>(context, e);
            }
            IMediatorResponse<TResult> result;
            try
            {
                var serializedResult = await response.Content.ReadAsStringAsync();
                result = _serializer.DeserializeResponse<TResult>(serializedResult);

                context.Status = result.Success ? ExecutionStatus.Succeeded : ExecutionStatus.Failed;
            }
            catch (Exception e)
            {
                context.Status = ExecutionStatus.Failed;
                return await ProcessParsingError<TResult>(context, response, e);
            }
            return await ProcessSuccessfullResult(context, response, result);
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessSuccessfullResult<TResult>(MediatorContext context, HttpResponseMessage response, IMediatorResponse<TResult> result)
        {
            return result != null
                ? Task.FromResult(result)
                : CreateErrorResponse<TResult>(context, $"No data received for action {context.ActionIdentifier}");
        }

        /// <summary>
        /// Error ocurred due to unexpected server response. Expected was JSON response with MediatorResponse structure.
        /// </summary>
        protected virtual Task<IMediatorResponse<TResult>> ProcessParsingError<TResult>(MediatorContext context, HttpResponseMessage response, Exception exception)
        {
            return CreateErrorResponse<TResult>(context, $"Can not deserialize response for action {context.ActionIdentifier}. ERROR: {exception.Message}, STATUS CODE: {(int)response.StatusCode} ({response.StatusCode})", exception);
        }

        /// <summary>
        /// Unexpected exception thrown during request sending. 
        /// Client was not able to receive response. 
        /// Can be caused by DNS issue, server timeout, network connection issue.
        /// </summary>
        protected virtual Task<IMediatorResponse<TResult>> ProcessRuntimeError<TResult>(MediatorContext context, Exception exception)
        {
            return CreateErrorResponse<TResult>(context, $"Error occured when executed action {context.ActionIdentifier}. ERROR: {exception.Message}", exception);
        }

        private Task<IMediatorResponse<TResult>> CreateErrorResponse<TResult>(MediatorContext context, string errorMessage, Exception? e = null)
        {
            _logger.LogError(e, errorMessage);
            IMediatorResponse<TResult> result = new MediatorResponse<TResult>(errorMessage,context.Action);
            return Task.FromResult(result);
        }
    }
}
