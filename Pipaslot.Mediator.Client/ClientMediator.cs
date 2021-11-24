using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
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
        private readonly static JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };
        private readonly HttpClient _httpClient;
        private readonly ClientMediatorOptions _options;
        private readonly IContractSerializer _serializer = new ContractSerializerV2();

        public ClientMediator(HttpClient httpClient, ClientMediatorOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<IMediatorResponse> Dispatch(IMediatorAction action, CancellationToken cancellationToken = default)
        {
            var contract = _serializer.CreateContract(action);

            var task = SendRequest<object>(contract, action, cancellationToken);
            return await task;
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IMediatorAction<TResult> action, CancellationToken cancellationToken = default)
        {
            var contract = _serializer.CreateContract(action);

            var task = SendRequest<TResult>(contract, action, cancellationToken);
            return await task;
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

        private async Task<IMediatorResponse<TResult>> SendRequest<TResult>(MediatorRequestSerializable contract, IMediatorAction action, CancellationToken cancellationToken = default)
        {
            var requestType = action.GetType();
            var url = _options.Endpoint + $"?type={requestType}";
            var json = _serializer.SerializeRequest(contract);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"); 
            content.Headers.Add(MediatorRequestSerializable.VersionHeader, MediatorRequestSerializable.VersionHeaderValueV2);
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                IMediatorResponse<TResult> result;
                try
                {
                    var serializedResult = await response.Content.ReadFromJsonAsync<MediatorResponseSerializableV2>(cancellationToken: cancellationToken);
                    result = _serializer.DeserializeResults<TResult>(serializedResult);
                }
                catch (Exception e)
                {
                    return await ProcessParsingError<TResult>(action, contract, response, e);
                }
                return await ProcessSuccessfullResult(action, contract, response, result);
            }
            else
            {
                return await ProcessUnsuccessfullStatusCode<TResult>(action, contract, response);
            }
        }

        protected virtual async Task<IMediatorResponse<TResult>> ProcessSuccessfullResult<TResult>(IMediatorAction action, MediatorRequestSerializable contract, HttpResponseMessage response, IMediatorResponse<TResult> result)
        {
            return await ProcessSuccessfullResult<TResult>(contract, action.GetType(), response, result);
        }

        protected virtual async Task<IMediatorResponse<TResult>> ProcessParsingError<TResult>(IMediatorAction action, MediatorRequestSerializable contract, HttpResponseMessage response, Exception e)
        {
            return await ProcessParsingError<TResult>(contract, action.GetType(), response, e);
        }

        protected virtual async Task<IMediatorResponse<TResult>> ProcessUnsuccessfullStatusCode<TResult>(IMediatorAction action, MediatorRequestSerializable contract, HttpResponseMessage response)
        {
            return await ProcessUnsuccessfullStatusCode<TResult>(contract, action.GetType(), response);
        }

        [Obsolete("User different method overload. This overload will be removed in version 4.0.0")]
        protected virtual Task<IMediatorResponse<TResult>> ProcessSuccessfullResult<TResult>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response, IMediatorResponse<TResult> result)
        {
            IMediatorResponse<TResult> normalized = result ?? throw new InvalidOperationException("No data received");
            return Task.FromResult(normalized);
        }

        [Obsolete("User different method overload. This overload will be removed in version 4.0.0")]
        protected virtual Task<IMediatorResponse<TResult>> ProcessParsingError<TResult>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response, Exception e)
        {
            IMediatorResponse<TResult> result = new MediatorResponse<TResult>("Can not deserialize response object");
            return Task.FromResult(result);
        }

        [Obsolete("User different method overload. This overload will be removed in version 4.0.0")]
        protected virtual Task<IMediatorResponse<TResult>> ProcessUnsuccessfullStatusCode<TResult>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response)
        {
            IMediatorResponse<TResult> result = new MediatorResponse<TResult>("Request failed");
            return Task.FromResult(result);
        }
    }
}
