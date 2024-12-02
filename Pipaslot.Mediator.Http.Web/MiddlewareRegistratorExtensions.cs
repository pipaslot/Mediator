using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Internal;
using Pipaslot.Mediator.Http.Web.Internal;
using Pipaslot.Mediator.Http.Web.Middlewares;
using Pipaslot.Mediator.Middlewares;
using System;

namespace Pipaslot.Mediator.Http.Web;

public static class MiddlewareRegistratorExtensions
{
    /// <inheritdoc cref="DirectHttpCallProtectionMiddleware"/>
    public static IMiddlewareRegistrator UseDirectHttpCallProtection(this IMiddlewareRegistrator config)
    {
        return config.Use<DirectHttpCallProtectionMiddleware>();
    }


    #region UseWhenDirectHttpCall

    /// <inheritdoc cref="UseWhenDirectHttpCall"/>
    public static IMiddlewareRegistrator UseWhenDirectHttpCall<TMiddleware>(this IMiddlewareRegistrator config)
        where TMiddleware : IMediatorMiddleware
    {
        return config.UseWhenDirectHttpCall(m => m.Use<TMiddleware>());
    }

    /// <summary>
    /// Applies Middlewares if the mediator action was invoked directly from any HTTP request. 
    /// It will not be applied to nested actions.
    /// <para>This is useful for cases of defining a demilitarized zone of an application where we want to secure all calls from outside but no longer need to re-verify security on internal or nested calls.</para>
    /// </summary>
    public static IMiddlewareRegistrator UseWhenDirectHttpCall(this IMiddlewareRegistrator config, Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return config.UseWhen((_, s) => IsFirstActionFromHttp(s), subMiddlewares);
    }

    #endregion

    #region UseWhenNotDirectHttpCall

    /// <inheritdoc cref="UseWhenNotDirectHttpCall"/>
    public static IMiddlewareRegistrator UseWhenNotDirectHttpCall<TMiddleware>(this IMiddlewareRegistrator config)
        where TMiddleware : IMediatorMiddleware
    {
        return config.UseWhenNotDirectHttpCall(m => m.Use<TMiddleware>());
    }

    /// <summary>
    /// Applies Middlewares if the mediator action was NOT invoked directly from any HTTP request or it was used as nested call.. 
    /// </summary>
    public static IMiddlewareRegistrator UseWhenNotDirectHttpCall(this IMiddlewareRegistrator config, Action<IMiddlewareRegistrator> subMiddlewares)
    {
        return config.UseWhen((a, s) => IsFirstActionFromHttp(s) == false, subMiddlewares);
    }

    #endregion

    /// <summary>
    /// Applies <see cref="AuthorizationMiddleware"/> if the mediator action was invoked directly from any HTTP request. 
    /// <para>For more details see: <see cref="UseWhenDirectHttpCall"/></para>
    /// </summary>
    public static IMiddlewareRegistrator UseAuthorizationWhenDirectHttpCall(this IMiddlewareRegistrator config)
    {
        return config.UseWhen((a, s) => IsFirstActionFromHttp(s), m => m.Use<AuthorizationMiddleware>(ServiceLifetime.Singleton));
    }

    private static bool IsFirstActionFromHttp(IServiceProvider sp)
    {
        var hca = sp.GetRequiredService<IHttpContextAccessor>();
        var mca = sp.GetRequiredService<IMediatorContextAccessor>();
        //var mop = sp.GetRequiredService<ServerMediatorOptions>();
        return mca.IsFirstAction() && hca.GetExecutionEndpoint(null) != HttpExecutionEndpoint.NoEndpoint;
    }
}