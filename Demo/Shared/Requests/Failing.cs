using Pipaslot.Mediator;

namespace Demo.Shared.Requests
{
    public static class Failing
    {
        public class Request : IRequest<Result>
        {

        }
        public class Message : IMessage
        {

        }

        public class Result
        {
        }
    }
}
