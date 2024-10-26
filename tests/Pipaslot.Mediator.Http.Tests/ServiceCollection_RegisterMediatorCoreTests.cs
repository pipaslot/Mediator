using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Http.Middlewares;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests
{
    public class ServiceCollection_RegisterMediatorCoreTests
    {
        [Fact]
        public void AddMediatorClient_WithoutConfiguration_RegisterMediatorCore()
        {
            TestMediatorExistence(c => c.AddMediatorClient());
        }

        [Fact]
        public void AddMediatorClient_WithConfiguration_RegisterMediatorCore()
        {
            TestMediatorExistence(c => c.AddMediatorClient(o => { }));
        }

        [Fact]
        public void AddMediatorClient_WithCustomDefaultHandler_RegisterMediatorCore()
        {
            TestMediatorExistence(c => c.AddMediatorClient<HttpClientExecutionMiddleware>());
        }

        [Fact]
        public void AddMediatorClient_WithCustomDefaultHandlerAndOptions_RegisterMediatorCore()
        {
            TestMediatorExistence(c => c.AddMediatorClient<HttpClientExecutionMiddleware>(c => { }));
        }

        [Fact]
        public void AddMediatorServer_WithoutConfiguration_RegisterMediatorCore()
        {
            TestMediatorExistence(c => c.AddMediatorServer());
        }

        [Fact]
        public void AddMediatorServer_WithConfiguration_RegisterMediatorCore()
        {
            TestMediatorExistence(c => c.AddMediatorServer(c => { }));
        }

        private void TestMediatorExistence(Action<ServiceCollection> setup)
        {
            var collection = new ServiceCollection();
            setup(collection);
            ;
            var services = collection.BuildServiceProvider();

            Assert.NotNull(services.GetRequiredService<IMediator>());
        }
    }
}