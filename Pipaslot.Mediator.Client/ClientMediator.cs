﻿using System;
using System.Linq;
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
        private static JsonSerializerOptions _serializationOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null
        };
        private readonly HttpClient _httpClient;
        private readonly ClientMediatorOptions _options;

        public ClientMediator(HttpClient httpClient, ClientMediatorOptions options)
        {
            _httpClient = httpClient;
            _options = options;
        }

        public async Task<IMediatorResponse> Dispatch(IMessage request, CancellationToken cancellationToken = default)
        {
            var contract = CreateContract(request);
            var requestType = request.GetType();

            var task = SendRequest<object>(contract, requestType, cancellationToken);
            return await task;
        }

        public async Task<IMediatorResponse<TResult>> Execute<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        {
            var contract = CreateContract(request);
            var requestType = request.GetType();

            var task = SendRequest<TResult>(contract, requestType, cancellationToken);
            return await task;
        }

        private async Task<IMediatorResponse<TResult>> SendRequest<TResult>(MediatorRequestSerializable contract, Type requestType, CancellationToken cancellationToken = default)
        {
            var url = _options.Endpoint + $"?type={requestType}";

            var content = JsonContent.Create(contract, null, _serializationOptions);
            content.Headers.Add(MediatorRequestSerializable.VersionHeader, MediatorRequestSerializable.VersionHeaderValueV2);
            var response = await _httpClient.PostAsync(url, content, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                IMediatorResponse<TResult> result;
                try
                {
                    var serializedResult = await response.Content.ReadFromJsonAsync<MediatorResponseSerializableV2>(cancellationToken: cancellationToken);
                    result = DeserializeResults<TResult>(serializedResult);
                }
                catch (Exception e)
                {
                    return await ProcessParsingError<TResult>(contract, requestType, response, e);
                }
                return await ProcessSuccessfullResult<TResult>(contract, requestType, response, result);
            }
            else
            {
                return await ProcessUnsuccessfullStatusCode<TResult>(contract, requestType, response);
            }
        }

        private IMediatorResponse<TResult> DeserializeResults<TResult>(MediatorResponseSerializableV2 serializedResult)
        {
            var results = serializedResult.Results
                .Select(r => DeserializeResult(r))
                .ToArray();
            return new MediatorResponseDeserialized<TResult>
            {
                Success = serializedResult.Success,
                ErrorMessages = serializedResult.ErrorMessages,
                Results = serializedResult.Results.Select(r => DeserializeResult(r)).ToArray()
            };
        }
        private object DeserializeResult(MediatorResponseSerializableV2.SerializedResult serializedResult)
        {
            var queryType = Type.GetType(serializedResult.ObjectName);
            if (queryType == null)
            {
                throw new Exception($"Can not recognize type {serializedResult.ObjectName} from received response");
            }
            var result = JsonSerializer.Deserialize(serializedResult.Json, queryType);
            if (result == null)
            {
                throw new Exception($"Can not deserialize contract as type {serializedResult.ObjectName} received from server");
            }
            return result;
        }

        private MediatorRequestSerializable CreateContract(object request)
        {
            return new MediatorRequestSerializable
            {
                Json = JsonSerializer.Serialize(request, _serializationOptions),
                ObjectName = request.GetType().AssemblyQualifiedName
            };
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessSuccessfullResult<TResult>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response, IMediatorResponse<TResult> result)
        {
            IMediatorResponse<TResult> normalized = result ?? throw new InvalidOperationException("No data received");
            return Task.FromResult(normalized);
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessParsingError<TResult>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response, Exception e)
        {
            IMediatorResponse<TResult> result = new MediatorResponse<TResult>("Can not deserialize response object");
            return Task.FromResult(result);
        }

        protected virtual Task<IMediatorResponse<TResult>> ProcessUnsuccessfullStatusCode<TResult>(MediatorRequestSerializable contract, Type requestType, HttpResponseMessage response)
        {
            IMediatorResponse<TResult> result = new MediatorResponse<TResult>("Request failed");
            return Task.FromResult(result);
        }
    }
}
