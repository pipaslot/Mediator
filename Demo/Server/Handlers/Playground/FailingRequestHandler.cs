using Demo.Shared.Playground;
using Pipaslot.Mediator;

namespace Demo.Server.Handlers.Playground;

public class FailingRequestHandler : IRequestHandler<Failing.Request, Failing.Result>
{
    public Task<Failing.Result> Handle(Failing.Request request, CancellationToken cancellationToken)
    {
        throw new Exception("Handler was not able to process REQUEST sucessfully");
    }
}