using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Moq;
using Pipaslot.Mediator.Http.Internal;
using Pipaslot.Mediator.Middlewares;

namespace Pipaslot.Mediator.Http.Tests.Internal;

public class HttpContextAccessorExtensionsTests
{
    [Fact]
    public void RootCall_WithMiddlewareFeatureSet_IsExecutedFromPublicApi()
    {
        var hca = CreateHttpContextAccessor(featureSet: true);
        var mca = CreateMediatorContextAccessor(contextStackCount: 1);

        Assert.True(HttpContextAccessorExtensions.IsExecutedFromPublicApi(hca, mca));
    }

    [Fact]
    public void RootCall_WithoutMiddlewareFeature_IsNotExecutedFromPublicApi()
    {
        // Simulates a call originating outside the ASP.NET Core request pipeline (e.g. a background service
        // holding a stale/ambient HttpContext), even though it is technically the first action on the stack.
        var hca = CreateHttpContextAccessor(featureSet: false);
        var mca = CreateMediatorContextAccessor(contextStackCount: 1);

        Assert.False(HttpContextAccessorExtensions.IsExecutedFromPublicApi(hca, mca));
    }

    [Fact]
    public void RootCall_WithoutHttpContext_IsNotExecutedFromPublicApi()
    {
        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(a => a.HttpContext).Returns((HttpContext)null!);
        var mca = CreateMediatorContextAccessor(contextStackCount: 1);

        Assert.False(HttpContextAccessorExtensions.IsExecutedFromPublicApi(accessor.Object, mca));
    }

    [Fact]
    public void NestedCall_WithMiddlewareFeatureSet_IsNotExecutedFromPublicApi()
    {
        var hca = CreateHttpContextAccessor(featureSet: true);
        var mca = CreateMediatorContextAccessor(contextStackCount: 2);

        Assert.False(HttpContextAccessorExtensions.IsExecutedFromPublicApi(hca, mca));
    }

    private static IHttpContextAccessor CreateHttpContextAccessor(bool featureSet)
    {
        var features = new FeatureCollection();
        if (featureSet)
        {
            features.Set(MediatorHttpContextFeature.Instance);
        }

        var context = new Mock<HttpContext>();
        context.SetupGet(c => c.Features).Returns(features);

        var accessor = new Mock<IHttpContextAccessor>();
        accessor.SetupGet(a => a.HttpContext).Returns(context.Object);
        return accessor.Object;
    }

    private static IMediatorContextAccessor CreateMediatorContextAccessor(int contextStackCount)
    {
        // Only the count matters to IsFirstAction(); the actual MediatorContext instances are irrelevant here
        // and cannot be constructed from this assembly (internal constructor).
        var accessor = new Mock<IMediatorContextAccessor>();
        accessor.SetupGet(a => a.ContextStack).Returns(new MediatorContext[contextStackCount]!);
        return accessor.Object;
    }
}
