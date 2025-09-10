using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Middlewares;
using Pipaslot.Mediator.Tests.ValidActions;
using System;
using System.Threading;

namespace Pipaslot.Mediator.Tests.Middlewares;

public class MediatorContextTests
{
    [Test]
    public void HasActionReturnValue_Message_ReturnsFalse()
    {
        var sut = CreateContext(new SingleHandler.Message(true));
        Assert.False(sut.HasActionReturnValue);
    }

    [Test]
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
        return new MediatorContext(mediator.Object, mcaMock.Object, spMock.Object, new ReflectionCache(), action, CancellationToken.None, null, null);
    }
}