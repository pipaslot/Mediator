using System;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.ValidActions;

public static class SingleHandler
{
    [ThreadStatic]
    public static int ExecutedCount;

    public class Request(bool pass) : IRequest<Response>
    {
        public bool Pass { get; } = pass;
    }

    public class Response
    {
        public static Response Instance = new();
    }

    public class Message(bool pass) : IMessage
    {
        public bool Pass { get; } = pass;
    }

    public class RequestException() : Exception(DefaultMessage)
    {
        public static string DefaultMessage = "Requesthandler failed";
    }

    public class MessageException() : Exception(DefaultMessage)
    {
        public static string DefaultMessage = "Message handler failed";
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