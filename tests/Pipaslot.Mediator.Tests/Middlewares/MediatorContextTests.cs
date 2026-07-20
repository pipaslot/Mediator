using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;

namespace Pipaslot.Mediator.Tests.Middlewares;

public class MediatorContextTests
{
    [Fact]
    public void HasActionReturnValue_Message_ReturnsFalse()
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        Assert.False(sut.HasActionReturnValue);
    }

    [Fact]
    public void HasActionReturnValue_Request_ReturnsTrue()
    {
        var sut = CreateContext(new SingleHandler.Request(true));
        Assert.True(sut.HasActionReturnValue);
    }

    [Fact]
    public void Depth_DefaultsToOne()
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        Assert.Equal(1, sut.Depth);
    }

    [Fact]
    public void IsNested_DefaultsToFalse()
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        Assert.False(sut.IsNested);
    }

    [Fact]
    public void SetDepth_One_IsNestedIsFalse()
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        sut.SetDepth(1);
        Assert.Equal(1, sut.Depth);
        Assert.False(sut.IsNested);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void SetDepth_GreaterThanOne_IsNestedIsTrue(int depth)
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        sut.SetDepth(depth);
        Assert.Equal(depth, sut.Depth);
        Assert.True(sut.IsNested);
    }

    [Fact]
    public void CopyEmpty_PreservesDepth()
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        sut.SetDepth(2);

        var copy = sut.CopyEmpty();

        Assert.Equal(2, copy.Depth);
        Assert.True(copy.IsNested);
    }

    private MediatorContext CreateContext(IMediatorAction action)
    {
        var mediator = new Mock<IMediator>();
        var spMock = new Mock<IServiceProvider>();
        var mcaMock = new Mock<IMediatorContextAccessor>();
        return new MediatorContext(mediator.Object, mcaMock.Object, spMock.Object, new ReflectionCache(), action, CancellationToken.None, null, null);
    }
}