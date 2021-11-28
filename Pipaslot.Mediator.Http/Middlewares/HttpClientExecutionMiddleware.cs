using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Options;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares
{
    public class HttpClientExecutionMiddleware : IExecutionMiddleware
    {
        public bool ExecuteMultipleHandlers => false;

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

        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            if (action is null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            if (action is IMediatorAction a)
            {
                try
                {
                    var response = await SendRequest<object>(a, cancellationToken);
                    context.Append(response);
                }
                catch (Exception e)
                {
                    context.ErrorMessages.Add(e.Message);
                }
            }
            else
            {
                throw new ArgumentException("Must implement interface " + nameof(IMediatorAction), nameof(action));
            }
        }

        protected virtual async Task<IMediatorResponse<TResult>> SendRequest<TResult>(IMediatorAction action, CancellationToken cancellationToken = default)
        {
            var actionName = action.GetType().ToString();
            try
            {
                var url = _options.Endpoint + $"?type={actionName}";
                var json = _serializer.SerializeRequest(action);
                var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content, cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    IMediatorResponse<TResult> result;
                    try
                    {
                        var serializedResult = await response.Content.ReadAsStringAsync();
                        result = _serializer.DeserializeResponse<TResult>(serializedResult);
                    }
                    catch (Exception e)
                    {
                        return await ProcessParsingError<TResult>(action, actionName, response, e);
                    }
                    return await ProcessSuccessfullResult(action, actionName, response, result);
                }
                else
                {
                    return await ProcessUnsuccessfullStatusCode<TResult>(action, actionName, response);
                }
            }
            catch (Exception e)
            {
                return await ProcessRuntimeError<TResult>(action, actionName, e);
            }
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessSuccessfullResult<TResult>(IMediatorAction action, string actionName, HttpResponseMessage response, IMediatorResponse<TResult> result)
        {
            return result != null
                ? Task.FromResult(result)
                : CreateErrorResponse<TResult>($"No data received for action {actionName}");
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessParsingError<TResult>(IMediatorAction action, string actionName, HttpResponseMessage response, Exception exception)
        {
            return CreateErrorResponse<TResult>($"Can not deserialize response for action {actionName}. ERROR: {exception.Message}", exception);
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessUnsuccessfullStatusCode<TResult>(IMediatorAction action, string actionName, HttpResponseMessage response)
        {
            return CreateErrorResponse<TResult>($"Request for action {actionName} failed with status code {((int)response.StatusCode)} ({response.StatusCode})");
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessRuntimeError<TResult>(IMediatorAction action, string actionName, Exception exception)
        {
            return CreateErrorResponse<TResult>($"Error occured when executed action {actionName}. ERROR: {exception.Message}", exception);
        }

        private Task<IMediatorResponse<TResult>> CreateErrorResponse<TResult>(string errorMessage, Exception? e = null)
        {
            _logger.LogError(e, errorMessage);
            IMediatorResponse<TResult> result = new MediatorResponse<TResult>(errorMessage);
            return Task.FromResult(result);
        }
    }
}
