using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using NSubstitute;
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
        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns((HttpContext)null!);
        var mca = CreateMediatorContextAccessor(contextStackCount: 1);

        Assert.False(HttpContextAccessorExtensions.IsExecutedFromPublicApi(accessor, mca));
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

        var context = Substitute.For<HttpContext>();
        context.Features.Returns(features);

        var accessor = Substitute.For<IHttpContextAccessor>();
        accessor.HttpContext.Returns(context);
        return accessor;
    }

    private static IMediatorContextAccessor CreateMediatorContextAccessor(int contextStackCount)
    {
        // Only the count matters to IsFirstAction(); the actual MediatorContext instances are irrelevant here
        // and cannot be constructed from this assembly (internal constructor).
        var accessor = Substitute.For<IMediatorContextAccessor>();
        accessor.ContextStack.Returns(new MediatorContext[contextStackCount]!);
        return accessor;
    }
}
