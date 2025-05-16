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
    public class MockMediatorContextAccessor : IMediatorContextAccessor
    {
        private readonly MediatorContext? _mediatorContext;

        public MockMediatorContextAccessor(MediatorContext? mediatorContext)
        {
            _mediatorContext = mediatorContext;
        }

        public MediatorContext MediatorContext => throw new NotImplementedException();

        public MediatorContext Context => throw new NotImplementedException();

        public IReadOnlyCollection<MediatorContext> ContextStack =>
            _mediatorContext is not null ? [_mediatorContext] : Array.Empty<MediatorContext>();
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
            ,features.Object
        );
    }

    [Fact]
    public void GetRootContextFeature_ShouldReturnNull_WhenRootContextIsNull()
    {
        var accessor = new MockMediatorContextAccessor(null);

        var result = accessor.GetRootContextFeature<FakeFeature>();

        Assert.Null(result);
    }

    [Fact]
    public void GetRootContextFeature_ShouldReturnFeature_WhenRootContextIsNotNull()
    {
        var featureValue = new FakeFeature();
        var featureCollectionMock = new Mock<IFeatureCollection>();
        featureCollectionMock.Setup(x => x.Get<FakeFeature>()).Returns(featureValue);
        var context = CreateContext(featureCollectionMock);
        var accessor = new MockMediatorContextAccessor(context);

        accessor.SetRootContextFeature(featureValue);
        var result = accessor.GetRootContextFeature<FakeFeature>();

        Assert.NotNull(result);
        Assert.Equal(featureValue, result);
    }

    [Fact]
    public void SetRootContextFeature_ShouldReturnFalse_WhenRootContextIsNull()
    {
        var accessor = new MockMediatorContextAccessor(null);
        var featureValue = new FakeFeature(); // Replace with an actual feature value

        var result = accessor.SetRootContextFeature(featureValue);

        Assert.False(result);
    }

    [Fact]
    public void SetRootContextFeature_ShouldReturnTrue_WhenRootContextIsNotNull()
    {
        var featureValue = new FakeFeature();
        var featureCollectionMock = new Mock<IFeatureCollection>();
        featureCollectionMock.Setup(x => x.Set(featureValue));
        var context = CreateContext(featureCollectionMock);
        var accessor = new MockMediatorContextAccessor(context);

        var result = accessor.SetRootContextFeature(featureValue);

        Assert.True(result);
        featureCollectionMock.VerifyAll();
    }
}