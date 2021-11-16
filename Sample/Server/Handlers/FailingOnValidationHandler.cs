using Pipaslot.Mediator;
using Sample.Shared.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Server.Handlers
{
    public class FailingOnValidationHandler : IRequestHandler<FailingOnValidation.Request, FailingOnValidation.Result>
    {
        public Task<FailingOnValidation.Result> Handle(FailingOnValidation.Request action, CancellationToken cancellationToken)
        {
            //This code will never be reached as the processing will be stopped by ValidatorMiddleware
            return Task.FromResult(new FailingOnValidation.Result());
        }
    }
}
