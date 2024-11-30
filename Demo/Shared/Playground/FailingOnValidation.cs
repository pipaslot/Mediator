using Pipaslot.Mediator;
using Pipaslot.Mediator.Authorization;

namespace Demo.Shared.Playground;

public static class FailingOnValidation
{
    [AnonymousPolicy]
    public class Request : IRequest<Result>, IValidable
    {
        public string[] Validate()
        {
            return ["Object validation failed on FAKE validation rule."];
        }
    }

    public class Result;
}