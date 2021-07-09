using Pipaslot.Mediator.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Client
{
    /// <summary>
    /// Request / message dispatcher over HTTP to server endpoint
    /// </summary>
    public class ClientMediator : IMediator
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ClientMediator> _logger;

        public ClientMediator(HttpClient httpClient, ILogger<ClientMediator> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IMediatorResponse> Dispatch(IMessage request, CancellationToken cancellationToken = default)
        {
            var contract = CreateContract(request);
            var requestType = request.GetType();

            var task = SendRequest<object>(contract, requestType, cancellationToken);
            return await task;
        }

        public async Task<IMediatorResponse<TResponse>> Execute<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var contract = CreateContract(request);
            var requestType = request.GetType();

            var task = SendRequest<TResponse>(contract, requestType, cancellationToken);
            return await task;
        }

        private async Task<IMediatorResponse<TResponse>> SendRequest<TResponse>(MediatorRequestSerializable contract, Type requestType, CancellationToken cancellationToken = default)
        {
            var (result, response) = await CallSendRequest<TResponse>(contract, requestType, cancellationToken);
            return await OnRequestPassed<TResponse>(result, response);
        }

        private async Task<(IMediatorResponse<TResponse>, HttpResponseMessage)> CallSendRequest<TResponse>(MediatorRequestSerializable contract, Type requestType, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = MediatorRequestSerializable.Endpoint + $"?type={requestType}";
                var response = await _httpClient.PostAsJsonAsync(url, contract, cancellationToken);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadFromJsonAsync<MediatorResponseDeserialized<TResponse>>(cancellationToken: cancellationToken);
                return (result, response);
            }
            catch (Exception e)
            {
                await OnRequestFailed(e, contract, requestType);

                return (new MediatorResponse<TResponse>(e.Message), null);
            }
        }

        private MediatorRequestSerializable CreateContract(object request)
        {
            return new MediatorRequestSerializable
            {
                Json = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }),
                ObjectName = request.GetType().AssemblyQualifiedName
            };
        }

        protected virtual Task<IMediatorResponse<TResponse>> OnRequestPassed<TResponse>(IMediatorResponse<TResponse> result, HttpResponseMessage response)
        {
            IMediatorResponse<TResponse> normalized = result ?? throw new InvalidOperationException("No data received");
            return Task.FromResult(normalized);
        }

        protected virtual Task OnRequestFailed(Exception e, MediatorRequestSerializable contract, Type requestType)
        {

            _logger.LogError(e, "Mediator request failed for " + requestType);
            return Task.CompletedTask;
        }
    }
}
