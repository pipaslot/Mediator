using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Pipaslot.Mediator.Contracts;
using Pipaslot.Mediator.Services;

namespace Pipaslot.Mediator.Server
{
    public class MediatorMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ServerMediatorOptions _option;

        public MediatorMiddleware(RequestDelegate next, ServerMediatorOptions option)
        {
            _next = next;
            _option = option;
        }

        public async Task Invoke(HttpContext context)
        {
            var method = context.Request.Method.ToUpper();
            if (context.Request.Path == _option.Endpoint && method == "POST")
            {
                var mediator = CreateMediator(context, method);
                var version = GetClientVersion(context);
                var contract = await GetContract(context, version);
                if (contract == null)
                {
                    throw new System.Exception($"Can not parse contract object from request body");
                }
                var executor = new RequestContractExecutor(mediator, version);
                var result = await executor.ExecuteQuery(contract, context.RequestAborted);
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(result);
            }
            else
            {
                await _next(context);
            }
        }

        private IMediator CreateMediator(HttpContext context, string httpMethod)
        {
            if (!(context.RequestServices.GetService(typeof(ServiceResolver)) is ServiceResolver resolver))
            {
                throw new System.Exception($"Interface {typeof(ServiceResolver).FullName} was not registered in service collection");
            }
            return new HttpMediator(resolver, httpMethod);
        }

        private string GetClientVersion(HttpContext context)
        {
            if (_option.KeepCompatibilityWithVersion1)
            {
                if (context.Request.Headers.TryGetValue(MediatorRequestSerializable.VersionHeader, out var version))
                {
                    return version;
                }
                return MediatorRequestSerializable.VersionHeaderValueV1;
            }
            return MediatorRequestSerializable.VersionHeaderValueV2;
        }

        private async Task<MediatorRequestSerializable?> GetContract(HttpContext context, string version)
        {
            var body = await GetBody(context);
            if (version == MediatorRequestSerializable.VersionHeaderValueV2)
            {
                return JsonSerializer.Deserialize<MediatorRequestSerializable>(body);
            }
            else
            {
                return JsonSerializer.Deserialize<MediatorRequestSerializable>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
        }

        private async Task<string> GetBody(HttpContext context)
        {
            using var reader = new StreamReader(context.Request.Body, Encoding.UTF8);
            var body = await reader.ReadToEndAsync();
            if (string.IsNullOrWhiteSpace(body))
            {
                throw new System.Exception("Request body has empty body. JSON was expected.");
            }
            return body;
        }
    }
}
