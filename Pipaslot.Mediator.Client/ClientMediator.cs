using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Serialization;

namespace Pipaslot.Mediator.Client
{
    /// <summary>
    /// Request / message dispatcher over HTTP to server endpoint
    /// </summary>
    public class ClientMediator : IMediator
    {
        private readonly HttpClient _httpClient;
        private readonly ClientMediatorOptions _options;
        private readonly IContractSerializer _serializer;

        public ClientMediator(HttpClient httpClient, ClientMediatorOptions options, IContractSerializer serializer)
        {
            _httpClient = httpClient;
            _options = options;
            _serializer = serializer;
        }

        public async Task<IMediatorResponse> Dispatch(IMediatorAction action, CancellationToken cancellationToken = default)
        {
            return await SendRequest<object>(action, cancellationToken);
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> action, CancellationToken cancellationToken = default)
        {
            return await SendRequest<TResult>(action, cancellationToken);
        }

        public async Task<TResult> ExecuteUnhandled<TResult>(IMediatorAction<TResult> action, CancellationToken cancellationToken = default)
        {
            var result = await Execute(action, cancellationToken);
            if (result.Failure)
            {
                throw new MediatorExecutionException(result);
            }
            if(result.Result == null)
            {
                throw new MediatorExecutionException($"Null was returned from mediator. Use method {nameof(Execute)} if you expect null as valid result.", result);
            }
            return result.Result;
        }

        public async Task DispatchUnhandled(IMediatorAction message, CancellationToken cancellationToken = default)
        {
            var result = await Dispatch(message, cancellationToken);
            if (result.Failure)
            {
                throw new MediatorExecutionException(result);
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
