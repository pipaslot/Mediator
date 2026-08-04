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

    // Public (not private) because NSubstitute must generate a Castle proxy for this type as the auto-value
    // for the unstubbed `Get<FakeFeature>()` call recorded while `.Returns(...)` is being configured.
    public record FakeFeature;

    // Built through the internal constructor rather than MediatorContext.Create, because these tests substitute the
    // whole IFeatureCollection to assert that the extension delegates to it. Create deliberately owns the collection
    // instead of accepting one, so that a context always carries the built-in feature defaults.
    private MediatorContext CreateContext(IFeatureCollection features)
    {
        return new MediatorContext(
            Substitute.For<IMediator>()
            , Substitute.For<IMediatorContextAccessor>()
            , Substitute.For<IServiceProvider>()
            , new ReflectionCache()
            , Substitute.For<IMediatorAction>()
            , CancellationToken.None
            , null
            , features
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
        var features = Substitute.For<IFeatureCollection>();
        features.Get<FakeFeature>().Returns(featureValue);
        var context = CreateContext(features);
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
        var features = Substitute.For<IFeatureCollection>();
        var context = CreateContext(features);
        var accessor = new MockMediatorContextAccessor(context);

        var result = accessor.SetRootContextFeature(featureValue);

        Assert.True(result);
        features.Received(1).Set(featureValue);
    }
}