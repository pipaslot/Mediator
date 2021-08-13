using Pipaslot.Mediator;
using Sample.Shared.Requests;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sample.Server.RequestHandlers
{
    public class FailingRequestHandler : IRequestHandler<Failing.Request, Failing.Result>
    {
        public Task<Failing.Result> Handle(Failing.Request request, CancellationToken cancellationToken)
        {
            throw new Exception("Handler was not able to process request sucessfully");
        }
    }
}
