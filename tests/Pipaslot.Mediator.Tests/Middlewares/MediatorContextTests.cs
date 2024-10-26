using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;
using Xunit;

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

    private MediatorContext CreateContext(IMediatorAction action)
    {
        var mediator = new Mock<IMediator>();
        var spMock = new Mock<IServiceProvider>();
        var mcaMock = new Mock<IMediatorContextAccessor>();
        return new MediatorContext(mediator.Object, mcaMock.Object, spMock.Object, action, CancellationToken.None, null, null);
    }
}