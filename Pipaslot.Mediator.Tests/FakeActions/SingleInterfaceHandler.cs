using Pipaslot.Mediator.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.FakeActions
{
    public static class SingleInterfaceHandler
    {
        public interface IRequestInterfaceAction : IRequest<Response>
        {
            bool Pass { get; }
        }
        public class RequestInterface1 : IRequestInterfaceAction
        {
            public bool Pass { get; }

            public RequestInterface1(bool pass)
            {
                Pass = pass;
            }
        }
        public class RequestInterface2 : IRequestInterfaceAction
        {
            public bool Pass { get; }

            public RequestInterface2(bool pass)
            {
                Pass = pass;
            }
        }
        public class RequestInterfaceForMediatorAction : IMediatorAction<Response>
        {
            public bool Pass { get; }

            public RequestInterfaceForMediatorAction(bool pass)
            {
                Pass = pass;
            }
        }
        public class Response
        {
            public static Response Instance = new();
        }

        public interface IMessageInterfaceAction : IMessage
        {
            bool Pass { get; }
        }

        public class MessageInterface1 : IMessageInterfaceAction
        {
            public bool Pass { get; }

            public MessageInterface1(bool pass)
            {
                Pass = pass;
            }
        }

        public class MessageInterface2 : IMessageInterfaceAction
        {
            public bool Pass { get; }

            public MessageInterface2(bool pass)
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

        public class RequestHandler : IRequestHandler<IRequestInterfaceAction, Response>
        {
            public Task<Response> Handle(IRequestInterfaceAction request, CancellationToken cancellationToken)
            {
                if (!request.Pass)
                {
                    throw new RequestException();
                }
                return Task.FromResult(Response.Instance);
            }
        }
        public class RequestInterfaceForMediatorActionHandler : IMediatorHandler<IMediatorAction<Response>, Response>
        {
            public Task<Response> Handle(IMediatorAction<Response> request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Response.Instance);
            }
        }
        
        public class MessageHandler : IMessageHandler<IMessageInterfaceAction>
        {
            public Task Handle(IMessageInterfaceAction request, CancellationToken cancellationToken)
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
