using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Client
{
    public class HttpClientExecutionMiddleware : IExecutionMiddleware
    {
        public bool ExecuteMultipleHandlers => false;

        private readonly HttpClient _httpClient;
        private readonly ClientMediatorOptions _options;
        private readonly IContractSerializer _serializer;

        public HttpClientExecutionMiddleware(HttpClient httpClient, ClientMediatorOptions options, IContractSerializer serializer)
        {
            _httpClient = httpClient;
            _options = options;
            _serializer = serializer;
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

        private async Task<IMediatorResponse<TResult>> SendRequest<TResult>(IMediatorAction action, CancellationToken cancellationToken = default)
        {
            var requestType = action.GetType();
            var url = _options.Endpoint + $"?type={requestType}";
            var json = _serializer.SerializeRequest(action, out var actionName);
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

        protected virtual Task<IMediatorResponse<TResult>> ProcessSuccessfullResult<TResult>(IMediatorAction action, string actionName, HttpResponseMessage response, IMediatorResponse<TResult> result)
        {
            IMediatorResponse<TResult> normalized = result ?? throw new InvalidOperationException("No data received");
            return Task.FromResult(normalized);
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessParsingError<TResult>(IMediatorAction action, string actionName, HttpResponseMessage response, Exception e)
        {
            IMediatorResponse<TResult> result = new MediatorResponse<TResult>("Can not deserialize response object");
            return Task.FromResult(result);
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessUnsuccessfullStatusCode<TResult>(IMediatorAction action, string actionName, HttpResponseMessage response)
        {
            IMediatorResponse<TResult> result = new MediatorResponse<TResult>("Request failed");
            return Task.FromResult(result);
        }
    }
}
