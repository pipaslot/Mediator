using System;
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

        public async Task<TResult> ExecuteUnhandled<TResult>(IRequest<TResult> request, CancellationToken cancellationToken = default)
        {
            var result = await Execute(request, cancellationToken);
            if (!result.Success)
            {
                throw new MediatorExecutionException(result.ErrorMessages);
            }
            if(result.Result == null)
            {
                throw new MediatorExecutionException($"Null was returned from mediator. Use method {nameof(Execute)} if you expect null as valid result.");
            }
            return result.Result;
        }

        public async Task DispatchUnhandled(IMessage message, CancellationToken cancellationToken = default)
        {
            var result = await Dispatch(message, cancellationToken);
            if (!result.Success)
            {
                throw new MediatorExecutionException(result.ErrorMessage);
            }
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
                queryType = Type.GetType(GetTypeWithoutAssembly(serializedResult.ObjectName));
                if (queryType == null)
                {
                    throw new Exception($"Can not recognize type {serializedResult.ObjectName} from received response");
                }
            }
            var result = JsonSerializer.Deserialize(serializedResult.Json, queryType);
            if (result == null)
            {
                throw new Exception($"Can not deserialize contract as type {serializedResult.ObjectName} received from server");
            }
            return result;
        }
        /// <summary>
        /// This method converst type Definition like "System.Collections.Generic.List`1[[MyType, MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], System.Private.CoreLib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=none"
        /// to "System.Collections.Generic.List`1[[MyType, MyAssembly, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]]"
        /// This is fix providing backward compatibility for .NET Core 3.1 and older where was issue with parsing types belonging to assembly System.Private.CoreLib, because this assembly is not available or the version might be different
        /// </summary>
        /// <param name="fullTypeAsString"></param>
        /// <returns></returns>
        internal static string GetTypeWithoutAssembly(string fullTypeAsString)
        {
            var isGeneric = fullTypeAsString.Contains("]]");
            if (isGeneric)
            {
                var startIndex = fullTypeAsString.IndexOf("[[")+2;
                var endIndex = fullTypeAsString.LastIndexOf("]]");
                var before = fullTypeAsString.Substring(0, startIndex);
                var between = fullTypeAsString.Substring(startIndex, endIndex-startIndex);
                var after = fullTypeAsString.Substring(endIndex);
                return before + GetTypeWithoutAssembly(between) + RemoveAssemblySuffix(after);
            }
            else
            {
                return RemoveAssemblySuffix(fullTypeAsString);
            }
            var genericIndex = fullTypeAsString.LastIndexOf("]], System.Private.CoreLib");
            var result = genericIndex >= 0
                ? fullTypeAsString.Substring(0, genericIndex + "]]".Length)
                : fullTypeAsString;


            if (genericIndex >= 0)
            {
                const int genericBracketEndLength = 2;
                return fullTypeAsString.Substring(0, genericIndex + genericBracketEndLength);
            }
            else
            {
                return RemoveAssemblySuffix(fullTypeAsString);
            }
        }
        private static string RemoveAssemblySuffix(string typeAsString)
        {
            var assemblyIndex = typeAsString.LastIndexOf(", System.Private.CoreLib");

            return assemblyIndex >=0 
                ? typeAsString.Substring(0, assemblyIndex)
                : typeAsString;
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
