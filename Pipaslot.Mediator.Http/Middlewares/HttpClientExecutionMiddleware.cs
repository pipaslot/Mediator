using Microsoft.Extensions.Logging;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Middlewares;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares;

/// <summary>
/// Middleware sending actions over HTTP client to mediator server implementation. No further middleware will be executed after this one.
/// </summary>
public class HttpClientExecutionMiddleware(
    HttpClient httpClient,
    ClientMediatorOptions options,
    IContractSerializer serializer,
    ILogger<HttpClientExecutionMiddleware> logger)
    : IExecutionMiddleware, IMediatorUrlFormatter
{
    private readonly ILogger _logger = logger;

    public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        var response = await SendRequest<object>(context).ConfigureAwait(false);
        context.Append(response);
    }

    public string FormatHttpGet(IMediatorAction action)
    {
        var serialized = serializer.SerializeRequest(action).Json;
        var decoded = WebUtility.UrlDecode(serialized);
        return $"{options.Endpoint}?{MediatorConstants.ActionQueryParamName}={decoded}";
    }

    protected virtual async Task<IMediatorResponse<TResult>> SendRequest<TResult>(MediatorContext context)
    {
        HttpResponseMessage response;
        try
        {
            var url = options.Endpoint + $"?type={context.ActionIdentifier}";
            using var content = GetHttpContent(context);
            response = await httpClient.PostAsync(url, content, context.CancellationToken).ConfigureAwait(false);
            // We do not check for successful status code.
            // It is completely up to server configuration what status code will be sent when action processing failed on server.
            // We just expect that server will return JSON in Mediator response format
            // Use ProcessParsingError for handling custom server responses and status codes 
        }
        catch (Exception ce) when (ce is OperationCanceledException or TaskCanceledException)
        {
            throw;
        }
        catch (Exception e)
        {
            context.Status = ExecutionStatus.Failed;
            return await ProcessRuntimeError<TResult>(context, e).ConfigureAwait(false);
        }

        IMediatorResponse<TResult> result;
        try
        {
            var serializedResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            result = serializer.DeserializeResponse<TResult>(serializedResult);

            context.Status = result.Success ? ExecutionStatus.Succeeded : ExecutionStatus.Failed;
        }
        catch (Exception e)
        {
            context.Status = ExecutionStatus.Failed;
            return await ProcessParsingError<TResult>(context, response, e).ConfigureAwait(false);
        }

        return await ProcessSuccessfullResult(context, response, result).ConfigureAwait(false);
    }

    private HttpContent GetHttpContent(MediatorContext context)
    {
        var serialized = serializer.SerializeRequest(context.Action);
        if (serialized.Streams.Any())
        {
            // We stream the content to the backend instead of sending as JSON (byte to JSON conversion may lead to out of memory exceptions)
            var content = new MultipartFormDataContent();
            // Add data/metadata sent together with the streams
            content.Add(new StringContent(serialized.Json, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json), MediatorConstants.MultipartFormDataJson);

            foreach (var pair in serialized.Streams)
            {
                var streamContent = new StreamContent(pair.Stream);
                content.Add(streamContent,"files",pair.Id);
            }

            return content;
        }
        // The contract was fully serialized into JSON, so we can send it as JSON
        return new StringContent(serialized.Json, System.Text.Encoding.UTF8, MediaTypeNames.Application.Json);
    }

    protected virtual Task<IMediatorResponse<TResult>> ProcessSuccessfullResult<TResult>(MediatorContext context, HttpResponseMessage response,
        IMediatorResponse<TResult> result)
    {
        return result != null
            ? Task.FromResult(result)
            : CreateErrorResponse<TResult>(context, $"No data received for action {context.ActionIdentifier}");
    }

    /// <summary>
    /// Error ocurred due to unexpected server response. Expected was JSON response with MediatorResponse structure.
    /// </summary>
    protected virtual Task<IMediatorResponse<TResult>> ProcessParsingError<TResult>(MediatorContext context, HttpResponseMessage response,
        Exception exception)
    {
        return CreateErrorResponse<TResult>(context,
            $"Can not deserialize response for action {context.ActionIdentifier}. ERROR: {exception.Message}, STATUS CODE: {(int)response.StatusCode} ({response.StatusCode})",
            exception);
    }

    /// <summary>
    /// Unexpected exception thrown during request sending. 
    /// Client was not able to receive response. 
    /// Can be caused by DNS issue, server timeout, network connection issue.
    /// </summary>
    protected virtual Task<IMediatorResponse<TResult>> ProcessRuntimeError<TResult>(MediatorContext context, Exception exception)
    {
        return CreateErrorResponse<TResult>(context, $"Error occured when executed action {context.ActionIdentifier}. ERROR: {exception.Message}",
            exception);
    }

    private Task<IMediatorResponse<TResult>> CreateErrorResponse<TResult>(MediatorContext context, string errorMessage, Exception? e = null)
    {
        _logger.LogError(e, errorMessage);
        IMediatorResponse<TResult> result = new MediatorResponse<TResult>(errorMessage, context.Action);
        return Task.FromResult(result);
    }
}