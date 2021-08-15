using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests
{
    public static class SingleHandler
    {
        public class Request : IRequest<Response>
        {
            public bool Pass { get; }

            public Request(bool pass)
            {
                Pass = pass;
            }
        }
        public class Response
        {
            public static Response Instance = new Response();
        }

        public class Message : IMessage
        {
            public bool Pass { get; }

            public Message(bool pass)
            {
                Pass = pass;
            }
        }

        public class RequestException : System.Exception
        {
            public static string Message = "REquesthandler failed";

            public RequestException() : base(Message)
            {
            }
        }

        public class MessageException : System.Exception
        {
            public static string Message = "Message handler failed";

            public MessageException() : base(Message)
            {
            }
        }

        public class RequestHandler : IRequestHandler<Request, Response>
        {
            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                if (!request.Pass)
                {
                    throw new RequestException();
                }
                return Task.FromResult(Response.Instance);
            }
        }

        public class MessageHandler : IMessageHandler<SingleHandler.Message>
        {
            public Task Handle(Message request, CancellationToken cancellationToken)
            {
                if (!request.Pass)
                {
                    throw new MessageException();
                }
                return Task.CompletedTask;
            }
        }
    }
}
