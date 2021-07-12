using System;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Abstractions;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Pipaslot.Mediator.Services;

namespace Pipaslot.Mediator.Tests
{
    public class ServiceResolverTest
    {
        [Fact]
        public void RequestDefinitionWithFixedResponse_ShouldResolve()
        {
            var services = CreateServiceProvider();
            var sut = services.GetRequiredService<ServiceResolver>();
            var handlers = sut.GetRequestHandlers<FakeFixedResponse>(typeof(FakeFixedRequest));

            Assert.Equal(1, handlers.Count());
            Assert.Equal(typeof(FakeFixedRequestHandler), handlers.First().GetType());
        }

        private IServiceProvider CreateServiceProvider()
        {
            var collection = new ServiceCollection();
            collection.AddMediator()
                .AddHandlersFromAssembly(this.GetType().Assembly);
            return collection.BuildServiceProvider();
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
