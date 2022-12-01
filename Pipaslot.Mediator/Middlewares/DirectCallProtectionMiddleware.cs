using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Middlewares
{
    /// <summary>
    /// Prevent direct calls for action which are not part of your application API
    /// </summary>
    public class DirectCallProtectionMiddleware : IMediatorMiddleware
    {
        private readonly IMediatorContextAccessor _contextAccessor;

        public DirectCallProtectionMiddleware(IMediatorContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public Task Invoke(MediatorContext context, MiddlewareDelegate next)
        {
            // We test for count to be 1 because the actual action is already counted
            if(_contextAccessor.ContextStack.Count == 1) { 
                throw MediatorException.CreateForForbidenDirectCall();
            }
            return next(context);
        }
    }
}
