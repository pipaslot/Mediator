using Pipaslot.Mediator.Middlewares;
using Sample.Shared;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Server
{
    public class ValidatorMiddleware : IMediatorMiddleware
    {
        public async Task Invoke<TAction>(TAction action, MediatorContext context, MiddlewareDelegate next, CancellationToken cancellationToken)
        {
            if(action is IValidable validable)
            {
                var errors = validable.Validate();
                if (errors != null && errors.Any())
                {
                    context.ErrorMessages.AddRange(errors);
                    return;
                }
            }
            await next(context);
        }
    }
}
