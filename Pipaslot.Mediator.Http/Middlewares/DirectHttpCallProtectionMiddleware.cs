using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Http.Internal;
using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares;

/// <summary>
/// Prevent direct calls for action which are not part of your application REST API. 
/// <para>Can be used as protection for queries placed in app demilitarized zone. Such a actions lacks authentication, authorization or different security checks.</para>
/// </summary>
/// <remarks>
/// Throws <see cref="MediatorException"/> when the action is the first, non-nested mediator call of an HTTP request that
/// already passed <see cref="MediatorMiddleware"/>. That covers a call sent to the mediator endpoint as well as one made from
/// a controller or minimal API endpoint placed behind <c>UseMediator()</c>. Nested calls from another handler or middleware,
/// and calls made outside of an HTTP request (e.g. a background service), are allowed. It is a guard for actions
/// that skip authentication or authorization because only trusted code calls them; it does not authenticate anyone, so
/// protect externally reachable actions with authorization policies instead. Register with
/// <see cref="MiddlewareRegistratorExtensions.UseDirectHttpCallProtection"/>.
/// </remarks>
public class DirectHttpCallProtectionMiddleware(IMediatorContextAccessor contextAccessor, IHttpContextAccessor httpContextAccessor)
    : IMediatorMiddleware
{
    /// <inheritdoc/>
    public Task Invoke(MediatorContext context, MiddlewareDelegate next)
    {
        if (HttpContextAccessorExtensions.IsExecutedFromPublicApi(httpContextAccessor, contextAccessor))
        {
            throw MediatorException.CreateForForbidenDirectCall();
        }

        return next(context);
    }
}