using System.Linq;
using System.Threading;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_DefineAndUseOwnRequstTypeWithFixedResultTests
{
    [Test]
    public void ShouldResolveRequest()
    {
        var handlerType = typeof(FakeFixedRequestHandler);
        var sut = Factory.CreateServiceProvider(c => c.AddHandlers([handlerType]));
        var handlers = sut.GetRequestHandlers(typeof(FakeFixedRequest), typeof(FakeFixedResponse));

        Assert.Single(handlers);
        Assert.Equal(handlerType, handlers.First().GetType());
    }

    public class FakeFixedResponse;

    public interface IFakeFixedRequest : IRequest<FakeFixedResponse>;

    public interface IFakeFixedRequestHandler<TRequest> : IRequestHandler<TRequest, FakeFixedResponse> where TRequest : IFakeFixedRequest;

    public class FakeFixedRequest : IFakeFixedRequest;

    public class FakeFixedRequestHandler : IFakeFixedRequestHandler<FakeFixedRequest>
    {
        public Task<FakeFixedResponse> Handle(FakeFixedRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FakeFixedResponse());
        }
    }
    
    [Test]
    public void ShouldResolveMessage()
    {
        var handlerType = typeof(FakeFixedMessageHandler);
        var sut = Factory.CreateServiceProvider(c => c.AddHandlers([handlerType]));
        var handlers = sut.GetMessageHandlers(typeof(FakeFixedMessage));

        Assert.Single(handlers);
        Assert.Equal(handlerType, handlers.First().GetType());
    }
    
    public interface IFakeFixedMessage : IMessage;

    public interface IFakeFixedMessageHandler<TMessage> : IMessageHandler<TMessage> where TMessage : IFakeFixedMessage;

    public class FakeFixedMessage : IFakeFixedMessage;

    public class FakeFixedMessageHandler : IFakeFixedMessageHandler<FakeFixedMessage>
    {
        public Task Handle(FakeFixedMessage Message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}