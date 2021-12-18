using Pipaslot.Mediator;
using Demo.Shared.Requests;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Server.Handlers
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
