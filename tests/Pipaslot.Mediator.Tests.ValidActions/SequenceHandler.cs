using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions
{
    public static class SequenceHandler
    {
        public static int ExecutedCount { get; set; }

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

        public class RequestHandler1 : IRequestHandler<Request, Response>, ISequenceHandler
        {
            public int Order => 1;

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

        public class RequestHandler2 : IRequestHandler<Request, Response>, ISequenceHandler
        {
            public int Order => 2;

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

        public class MessageHandler1 : IMessageHandler<Message>, ISequenceHandler
        {
            public int Order => 1;

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

        public class MessageHandler2 : IMessageHandler<Message>, ISequenceHandler
        {
            public int Order => 2;

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