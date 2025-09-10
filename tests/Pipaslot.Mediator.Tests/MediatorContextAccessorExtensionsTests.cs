using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Middlewares.Features;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Pipaslot.Mediator.Tests;

public class MediatorContextAccessorExtensionsTests
{
    public class MockMediatorContextAccessor(MediatorContext? mediatorContext) : IMediatorContextAccessor
    {
        public MediatorContext MediatorContext => throw new NotImplementedException();

        public MediatorContext Context => throw new NotImplementedException();

        public IReadOnlyCollection<MediatorContext> ContextStack =>
            mediatorContext is not null ? [mediatorContext] : Array.Empty<MediatorContext>();
    }

    private record FakeFeature;

    private MediatorContext CreateContext(Mock<IFeatureCollection> features)
    {
        return new MediatorContext(
            new Mock<IMediator>().Object
            , new Mock<IMediatorContextAccessor>().Object
            , new Mock<IServiceProvider>().Object
            , new ReflectionCache()
            , new Mock<IMediatorAction>().Object
            , CancellationToken.None
            , null
            , features.Object
        );
    }

    [Test]
    public async Task GetRootContextFeature_ShouldReturnNull_WhenRootContextIsNull()
    {
        var accessor = new MockMediatorContextAccessor(null);

        var result = accessor.GetRootContextFeature<FakeFeature>();

        await Assert.That(result).IsNull();
    }

    [Test]
    public async Task GetRootContextFeature_ShouldReturnFeature_WhenRootContextIsNotNull()
    {
        var featureValue = new FakeFeature();
        var featureCollectionMock = new Mock<IFeatureCollection>();
        featureCollectionMock.Setup(x => x.Get<FakeFeature>()).Returns(featureValue);
        var context = CreateContext(featureCollectionMock);
        var accessor = new MockMediatorContextAccessor(context);

        accessor.SetRootContextFeature(featureValue);
        var result = accessor.GetRootContextFeature<FakeFeature>();

        await Assert.That(result).IsNotNull();
        await Assert.That(result).IsEqualTo(featureValue);
    }

    [Test]
    public async Task SetRootContextFeature_ShouldReturnFalse_WhenRootContextIsNull()
    {
        var accessor = new MockMediatorContextAccessor(null);
        var featureValue = new FakeFeature();

        var result = accessor.SetRootContextFeature(featureValue);

        await Assert.That(result).IsFalse();
    }

    [Test]
    public async Task SetRootContextFeature_ShouldReturnTrue_WhenRootContextIsNotNull()
    {
        var featureValue = new FakeFeature();
        var featureCollectionMock = new Mock<IFeatureCollection>();
        featureCollectionMock.Setup(x => x.Set(featureValue));
        var context = CreateContext(featureCollectionMock);
        var accessor = new MockMediatorContextAccessor(context);

        var result = accessor.SetRootContextFeature(featureValue);

        await Assert.That(result).IsTrue();
        featureCollectionMock.VerifyAll();
    }
}