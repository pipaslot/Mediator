﻿using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Pipaslot.Mediator.Contracts;

namespace Pipaslot.Mediator.Client
{
    /// <summary>
    /// Request / message dispatcher over HTTP to server endpoint
    /// </summary>
    public class ClientMediator : IMediator
    {
        private readonly HttpClient _httpClient;

        public ClientMediator(HttpClient httpClient)
        {
            _httpClient = httpClient;
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
            var url = MediatorRequestSerializable.Endpoint + $"?type={requestType}";
            var response = await _httpClient.PostAsJsonAsync(url, contract, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                IMediatorResponse<TResponse> result;
                try
                {
                    result = await response.ParseResponse<TResponse>(cancellationToken);
                }
                catch (Exception e)
                {
                    return await ProcessParsingError<TResponse>(contract, requestType, response, e);
                }
                return await ProcessSuccessfullResult<TResponse>(contract, requestType, response, result);
            }
            else
            {
                return await ProcessUnsuccessfullStatusCode<TResponse>(contract, requestType, response);
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

        protected virtual Task<IMediatorResponse<TResponse>> ProcessSuccessfullResult<TResponse>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response, IMediatorResponse<TResponse> result)
        {
            IMediatorResponse<TResponse> normalized = result ?? throw new InvalidOperationException("No data received");
            return Task.FromResult(normalized);
        }

        protected virtual Task<IMediatorResponse<TResponse>> ProcessParsingError<TResponse>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response, Exception e)
        {
            IMediatorResponse<TResponse> result = new MediatorResponse<TResponse>("Can not deserialize response object");
            return Task.FromResult(result);
        }

        protected virtual Task<IMediatorResponse<TResponse>> ProcessUnsuccessfullStatusCode<TResponse>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response)
        {
            IMediatorResponse<TResponse> result = new MediatorResponse<TResponse>("Request failed");
            return Task.FromResult(result);
        }
    }
}
