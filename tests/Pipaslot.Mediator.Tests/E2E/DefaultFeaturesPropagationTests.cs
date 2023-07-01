using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using Pipaslot.Mediator.Tests.ValidActions;
using System.Threading.Tasks;

namespace Pipaslot.Mediator.Tests.E2E
{
    public class DefaultFeaturesPropagationTests
    {
        [Fact]
        public async Task Executed()
        {
            var (sut, features) = SetupMediator();
            var res = await sut.Execute(new SingleHandler.Request(true), defaultFeatures: features);
            Assert.True(res.Success);
        }

        [Fact]
        public async Task ExecutedUnhandled()
        {
            var (sut, features) = SetupMediator();
            await sut.ExecuteUnhandled(new SingleHandler.Request(true), defaultFeatures: features);
        }

        [Fact]
        public async Task Dispatch()
        {
            var (sut, features) = SetupMediator();
            var res = await sut.Dispatch(new SingleHandler.Message(true), defaultFeatures: features);
            Assert.True(res.Success);
        }

        [Fact]
        public async Task DispatchUnhandled()
        {
            var (sut, features) = SetupMediator();
            await sut.DispatchUnhandled(new SingleHandler.Message(true), defaultFeatures:features);
        }

        private (IMediator Mediator, FeatureCollection Features) SetupMediator()
        {
            var mediator = Factory.CreateConfiguredMediator(c =>
            {
                c.Use<AssertCustomFeatureExistenceMiddleware>();
            });
            var features = new FeatureCollection();
            features.Set(new CustomFeature());
            return (mediator, features);
        }

        private class CustomFeature
        {
            public string Name { get; set; } = "name";
        }

        private class AssertCustomFeatureExistenceMiddleware : IMediatorMiddleware
        {
            public async Task Invoke(MediatorContext context, MiddlewareDelegate next)
            {
                Assert.NotNull(context.Features.Get<CustomFeature>());
                await next(context);
            }
        }
    }
}
