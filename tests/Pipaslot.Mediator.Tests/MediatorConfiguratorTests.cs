using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using System;

namespace Pipaslot.Mediator.Tests;

public class MediatorConfiguratorTests
{
    [Theory]
    [InlineData(typeof(SingleHandler.Message), true)]
    [InlineData(typeof(NonActionType), false)]
    [InlineData(typeof(SingleHandler.Request), true)]
    public void AddActions_ActionTypePassed(Type type, bool shouldPass)
    {
        var sut = Create();
        if (shouldPass)
        {
            sut.AddActions([type]);
        }
        else
        {
            Assert.Throws<MediatorException>(() =>
            {
                sut.AddActions([type]);
            });
        }
    }

    [Fact]
    public void AddHandlers_NonHandlerTypePassed_ThrowException()
    {
        var sut = Create();
        Assert.Throws<MediatorException>(() =>
        {
            sut.AddHandlers([typeof(object)]);
        });
    }

    [Fact]
    public void AddHandlers_HandlerTypePassed_Pass()
    {
        var sut = Create();
        sut.AddHandlers([typeof(NopMesageHandler)]);
    }

    private MediatorConfigurator Create()
    {
        var sc = new Mock<IServiceCollection>();
        return new MediatorConfigurator(sc.Object);
    }

    private class NonActionType;
}