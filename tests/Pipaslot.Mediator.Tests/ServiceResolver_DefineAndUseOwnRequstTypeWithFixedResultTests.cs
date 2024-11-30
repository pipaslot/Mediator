using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests;

public class ServiceResolver_DefineAndUseOwnRequstTypeWithFixedResultTests
{
    [Fact]
    public void ShouldResolve()
    {
        var handlerType = typeof(FakeFixedRequestHandler);
        var sut = Factory.CreateServiceProvider(c => c.AddHandlers([handlerType]));
        var handlers = sut.GetRequestHandlers(typeof(FakeFixedRequest), typeof(FakeFixedResponse));

        Assert.Single(handlers);
        Assert.Equal(handlerType, handlers.First().GetType());
    }

    public class FakeFixedResponse
    {
    }

    public interface IFakeFixedRequest : IRequest<FakeFixedResponse>
    {
    }

    public interface IFakeFixedRequestHandler<TRequest> : IRequestHandler<TRequest, FakeFixedResponse> where TRequest : IFakeFixedRequest
    {
    }

    public class FakeFixedRequest : IFakeFixedRequest
    {
    }

    public class FakeFixedRequestHandler : IFakeFixedRequestHandler<FakeFixedRequest>
    {
        public Task<FakeFixedResponse> Handle(FakeFixedRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new FakeFixedResponse());
        }
    }
}