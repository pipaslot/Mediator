using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Notifications;
using Pipaslot.Mediator.Tests.ValidActions;
using System;

namespace Pipaslot.Mediator.Tests.Middlewares;

/// <summary>
/// Behavior every <see cref="MediatorContext"/> has once it exists, regardless of how it was built - result and
/// exception recording, depth, and what the action type implies. How a context created by
/// <see cref="MediatorContext.Create"/> fills the dependencies the pipeline would otherwise supply belongs to
/// <see cref="MediatorContext_CreateTests"/>.
/// </summary>
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

    [Fact]
    public void AddException_SetsStatusFailed()
    {
        var sut = CreateContext(new SingleHandler.Message(true));

        sut.AddException(new InvalidOperationException("boom"));

        Assert.Equal(ExecutionStatus.Failed, sut.Status);
    }

    [Fact]
    public void AddException_AddsToExceptions()
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        var ex = new InvalidOperationException("boom");

        sut.AddException(ex);

        Assert.Same(ex, Assert.Single(sut.Exceptions));
    }

    [Fact]
    public void AddException_DoesNotAddToResults()
    {
        var sut = CreateContext(new SingleHandler.Message(true));

        sut.AddException(new InvalidOperationException("boom"));

        Assert.Empty(sut.Results);
    }

    [Fact]
    public void AddException_DoesNotCreateNotification()
    {
        var sut = CreateContext(new SingleHandler.Message(true));

        sut.AddException(new InvalidOperationException("boom"));

        Assert.DoesNotContain(sut.Results, r => r is Notification);
    }

    [Fact]
    public void AddException_CalledTwice_BothRecordedInOrder()
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        var first = new InvalidOperationException("first");
        var second = new ArgumentException("second");

        sut.AddException(first);
        sut.AddException(second);

        Assert.Equal([first, second], sut.Exceptions);
    }

    private MediatorContext CreateContext(IMediatorAction action)
    {
        return MediatorContext.Create(action);
    }
}