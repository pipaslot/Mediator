using Microsoft.AspNetCore.Http;
using Pipaslot.Mediator.Middlewares;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Http.Middlewares
{
    /// <summary>
    /// Prevent direct calls for action which are not part of your application API
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
            // We test for count to be 1 because the actual action is already counted
            if (_contextAccessor.ContextStack.Count == 1 && _httpContextAccessor.HttpContext != null)
            {
                throw MediatorException.CreateForForbidenDirectCall();
            }
            return next(context);
        }
    }
}
