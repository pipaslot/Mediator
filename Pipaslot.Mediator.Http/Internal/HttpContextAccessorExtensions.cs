using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Http.Configuration;

namespace Pipaslot.Mediator.Http.Internal
{
    internal static class HttpContextAccessorExtensions
    {
        internal static HttpExecutionEndpoint GetExecutionEndpoint(this IHttpContextAccessor accessor, ServerMediatorOptions? options)
        {
            var context = accessor.HttpContext;
            if (context == null)
            {
                return HttpExecutionEndpoint.NoEndpoint;
            }
            if(options != null && context.Request.Path == options.Endpoint)
            {
                return HttpExecutionEndpoint.MediatorEndpoint;
            }
            return HttpExecutionEndpoint.CustomEndpoint;
        }
    }
}
