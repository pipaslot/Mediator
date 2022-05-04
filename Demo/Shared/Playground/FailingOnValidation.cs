using Pipaslot.Mediator;

namespace Demo.Shared.Playground
{
    public static class FailingOnValidation
    {
        public class Request : IRequest<Result>, IValidable
        {
            public string[] Validate()
            {
                return new string[] { "Object validation failed on FAKE validation rule." };
            }
        }

        public class Result
        {
        }
    }
}
