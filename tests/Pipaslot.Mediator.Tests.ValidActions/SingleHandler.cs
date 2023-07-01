using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions
{
    public static class SingleHandler
    {
        [ThreadStatic]
        public static int ExecutedCount;
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
            public static Response Instance = new();
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
            public static string DefaultMessage = "Requesthandler failed";

            public RequestException() : base(DefaultMessage)
            {
            }
        }

        public class MessageException : System.Exception
        {
            public static string DefaultMessage = "Message handler failed";

            public MessageException() : base(DefaultMessage)
            {
            }
        }

        public class RequestHandler : IRequestHandler<Request, Response>
        {
            public Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                ExecutedCount++;
                if (!request.Pass)
                {
                    throw new RequestException();
                }
                return Task.FromResult(Response.Instance);
            }
        }

        public class MessageHandler : IMessageHandler<Message>
        {
            public Task Handle(Message request, CancellationToken cancellationToken)
            {
                ExecutedCount++;
                if (!request.Pass)
                {
                    throw new MessageException();
                }
                return Task.CompletedTask;
            }
        }
    }
}
