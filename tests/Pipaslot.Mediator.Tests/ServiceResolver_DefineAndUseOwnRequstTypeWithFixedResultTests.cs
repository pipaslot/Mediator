using Pipaslot.Mediator.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Tests
{
    public class ServiceResolver_DefineAndUseOwnRequstTypeWithFixedResultTests
    {
        [Fact]
        public void ShouldResolve()
        {
            var sut = Factory.CreateServiceProvider(c => c.AddHandlersFromAssembly(this.GetType().Assembly));
            var handlers = sut.GetRequestHandlers(typeof(FakeFixedRequest), typeof(FakeFixedResponse));

            Assert.Single(handlers);
            Assert.Equal(typeof(FakeFixedRequestHandler), handlers.First().GetType());
        }

        public class FakeFixedResponse { }
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
}
