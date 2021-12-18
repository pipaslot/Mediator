using Pipaslot.Mediator;
using Demo.Shared.Requests;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Demo.Server.Handlers
{
    public class FailingRequestHandler : IRequestHandler<Failing.Request, Failing.Result>
    {
        public Task<Failing.Result> Handle(Failing.Request request, CancellationToken cancellationToken)
        {
            throw new Exception("Handler was not able to process REQUEST sucessfully");
        }
    }
}
