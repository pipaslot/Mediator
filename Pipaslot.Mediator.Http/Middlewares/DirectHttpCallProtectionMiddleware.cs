using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Http.Internal;
using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares
{
    /// <summary>
    /// Prevent direct calls for action which are not part of your application REST API. 
    /// <para>Can be used as protection for queries placed in app demilitarized zone. Such a actions lacks authentication, authorization or different security checks.</para>
    /// </summary>
    public class DirectHttpCallProtectionMiddleware : IMediatorMiddleware
    {
        private readonly IMediatorContextAccessor _contextAccessor;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DirectHttpCallProtectionMiddleware(IMediatorContextAccessor contextAccessor, IHttpContextAccessor httpContextAccessor)
        {
            _contextAccessor = contextAccessor;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            if (IsApplicable(_contextAccessor, _httpContextAccessor))
            {
                throw MediatorException.CreateForForbidenDirectCall();
            }
            return next(context);
        }

        internal static bool IsApplicable(IMediatorContextAccessor contextAccessor, IHttpContextAccessor httpContextAccessor)
        {
            return contextAccessor.IsFirstAction() && httpContextAccessor.GetExecutionEndpoint(null) != HttpExecutionEndpoint.NoEndpoint;
        }
    }
}
