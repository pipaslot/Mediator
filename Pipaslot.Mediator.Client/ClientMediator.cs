using Pipaslot.Mediator.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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

        private readonly Dictionary<int, Task> _queryTaskCache = new Dictionary<int, Task>();
        private readonly object _queryTaskCacheLock = new object();

        public ClientMediator(HttpClient httpClient, ILogger<ClientMediator> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<IMediatorResponse> Dispatch(IMessage request, CancellationToken cancellationToken = default)
        {
            var contract = CreateContract(request);
            var requestType = request.GetType();

            var hashCode = (contract.Json, contract.ObjectName).GetHashCode();
            try
            {
                var task = GetRequestTaskFromCacheOrCreateNewRequest<object>(hashCode, contract, requestType, cancellationToken);
                return await task;
            }
            finally
            {
                lock (_queryTaskCacheLock)
                {
                    _queryTaskCache.Remove(hashCode);
                }
            }
        }

        public async Task<IMediatorResponse<TResponse>> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var contract = CreateContract(request);
            var requestType = request.GetType();

            var hashCode = (contract.Json, contract.ObjectName).GetHashCode();
            try
            {
                var task = GetRequestTaskFromCacheOrCreateNewRequest<TResponse>(hashCode, contract, requestType, cancellationToken);
                return await task;
            }
            finally
            {
                lock (_queryTaskCacheLock)
                {
                    _queryTaskCache.Remove(hashCode);
                }
            }
        }

        private Task<IMediatorResponse<TResponse>> GetRequestTaskFromCacheOrCreateNewRequest<TResponse>(int hashCode, MediatorRequestSerializable contract, Type requestType, CancellationToken cancellationToken = default)
        {
            lock (_queryTaskCacheLock)
            {
                if (_queryTaskCache.TryGetValue(hashCode, out var task))
                {
                    return (Task<IMediatorResponse<TResponse>>)task;
                }
                var newTask = SendRequest<TResponse>(contract, requestType, cancellationToken);
                _queryTaskCache[hashCode] = newTask;
                return newTask;
            }
        }

        private async Task<IMediatorResponse<TResponse>> SendRequest<TResponse>(MediatorRequestSerializable contract, Type requestType, CancellationToken cancellationToken = default)
        {
            try
            {
                var url = MediatorRequestSerializable.Endpoint + $"?type={requestType}";
                var response = await _httpClient.PostAsJsonAsync(url, contract, cancellationToken);
                response.EnsureSuccessStatusCode();

                var result = await response.Content.ReadFromJsonAsync<MediatorResponseDeserialized<TResponse>>(cancellationToken: cancellationToken);
                return result?? throw new InvalidOperationException("No data received");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Mediator request failed for "+ requestType);
                return new MediatorResponse<TResponse>(e.Message);
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
    }
}
