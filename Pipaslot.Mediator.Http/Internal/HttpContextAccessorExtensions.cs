using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Pipaslot.Mediator.Http.Internal;

internal static class HttpContextAccessorExtensions
{
    /// <inheritdoc cref="IsExecutedFromPublicApi(IHttpContextAccessor, IMediatorContextAccessor)"/>
    internal static bool IsExecutedFromPublicApi(this IServiceProvider serviceProvider)
    {
        var hca = serviceProvider.GetRequiredService<IHttpContextAccessor>();
        var mca = serviceProvider.GetRequiredService<IMediatorContextAccessor>();
        return IsExecutedFromPublicApi(hca, mca);
    }
    
    /// <summary>
    /// Detect if the processed Mediator call is Executed from a public API.
    /// That request has to be the first mediator call (when nesting) and needs to be invoked by the MediatorMiddleware or by some other middleware after (Controllers, Minimal APIs...)
    /// </summary>
    internal static bool IsExecutedFromPublicApi(IHttpContextAccessor hca, IMediatorContextAccessor mca)
    {
        return mca.IsFirstAction() && hca.WasMediatorMiddlewareExecuted();
    }
    
    /// <summary>
    /// Detect where was mediator call executed from.
    /// Returns FALSE when executed out of HTTP request (from background services)
    /// Returns TRUE when mediator middleware was already executed in the .net core pipeline. Since that moment we can consider the mediator call as incoming from the application API.
    /// </summary>
    /// <param name="accessor"></param>
    /// <returns></returns>
    private static bool WasMediatorMiddlewareExecuted(this IHttpContextAccessor accessor)
    {
        var context = accessor.HttpContext;
        return context is not null && context.Features.Get<MediatorHttpContextFeature>() != null;
    }
}