using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Internal;
using Pipaslot.Mediator.Http.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http;

public class MediatorMiddleware(RequestDelegate next, ServerMediatorOptions option, IContractSerializer serializer)
{
    public async Task Invoke(HttpContext context)
    {
        context.Features.Set(new MediatorHttpContextFeature());

        var method = context.Request.Method.ToUpper();
        var isPost = method == "POST";
        var isGet = method == "GET";
        if (context.Request.Path == option.Endpoint && (isPost || isGet))
        {
            var mediatorResponse = await SafeExecute(context, isPost).ConfigureAwait(false);
            var serializedResponse = serializer.SerializeResponse(mediatorResponse);
            // Change status code only if has default value (200: OK)
            if (context.Response.StatusCode == (int)HttpStatusCode.OK && mediatorResponse.Failure)
            {
                context.Response.StatusCode = option.ErrorHttpStatusCode;
            }

            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(serializedResponse).ConfigureAwait(false);
            }
        }
        else
        {
            await next(context).ConfigureAwait(false);
        }
    }

    private async Task<IMediatorResponse> SafeExecute(HttpContext context, bool isPost)
    {
        try
        {
            IMediatorAction action;
            var mediator = CreateMediator(context);
            if (!context.Request.HasFormContentType)
            {
                // Standard JSON expected body or HTTP GET
                //TODO: read body as stream
                var body = isPost
                    ? await GetBody(context).ConfigureAwait(false)
                    : context.Request.Query.TryGetValue(MediatorConstants.ActionQueryParamName, out var actionQuery)
                        ? actionQuery.ToString()
                        : "";
                action = serializer.DeserializeRequest(body, []);
            }
            else
            {
                // body was sent as Multipart form-data
                var form = await context.Request.ReadFormAsync(context.RequestAborted).ConfigureAwait(false);

                // Get json metadata
                var jsonPart = form[MediatorConstants.MultipartFormDataJson];
                var streams = new List<StreamContract>();
                foreach (var file in form.Files)
                {
                    streams.Add(new StreamContract(file.FileName, file.OpenReadStream()));
                }

                action = serializer.DeserializeRequest(jsonPart, streams);
            }

            var mediatorResponse = action is IMediatorActionProvidingData req
                ? await ExecuteRequest(mediator, req, context.RequestAborted).ConfigureAwait(false)
                : await ExecuteMessage(mediator, action, context.RequestAborted).ConfigureAwait(false);
            return mediatorResponse;
        }
        catch (Exception ex)
        {
            return new MediatorResponse(ex.Message);
        }
    }

    private static IMediator CreateMediator(HttpContext context)
    {
        if (context.RequestServices.GetService(typeof(IMediator)) is not IMediator resolver)
        {
            throw MediatorHttpException.CreateForUnregisteredService(typeof(IMediator));
        }

        return resolver;
    }

    private static async Task<string> GetBody(HttpContext context)
    {
        using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
        var body = await reader.ReadToEndAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(body))
        {
            throw MediatorHttpException.CreateForInvalidRequest(body);
        }

        return body;
    }

    private static async Task<IMediatorResponse> ExecuteMessage(IMediator mediator, IMediatorAction message, CancellationToken cancellationToken)
    {
        try
        {
            return await mediator.Dispatch(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            // This should never happen because mediator handles errors internally. But need to prevent errors if somebody will override mediator behavior
            return new MediatorResponse(e.Message, message);
        }
    }

    private async Task<IMediatorResponse> ExecuteRequest(IMediator mediator, object query, CancellationToken cancellationToken)
    {
        var resultType = RequestGenericHelpers.GetRequestResultType(query.GetType())
                         ?? throw new MediatorHttpException($"Object {query.GetType()} is not assignable to type {typeof(IMediatorAction<>)}");
        var method = mediator.GetType()
            .GetMethod(nameof(IMediator.Execute))!
            .MakeGenericMethod(resultType);
        try
        {
            var task = (Task)method.Invoke(mediator, [query, cancellationToken])!;
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            var result = resultProperty?.GetValue(task);
            if (result is MediatorResponse mediatorResponse)
            {
                return mediatorResponse;
            }

            return new MediatorResponse(
                $"Unexpected result type from mediator pipeline. Was expected {typeof(MediatorResponse)} but {result?.GetType()} was returned instead.");
        }
        catch (Exception e)
        {
            // This should never happen because mediator handles errors internally. But need to prevent errors if somebody will override mediator behavior
            return new MediatorResponse(e.Message);
        }
    }
}