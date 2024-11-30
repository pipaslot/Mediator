using Microsoft.Extensions.DependencyInjection;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Tests.ValidActions;
using System;

namespace Pipaslot.Mediator.Tests;

public class MediatorConfiguratorTests
{
    [Fact]
    public void AddActions_NonActionTypePassed_ThrowException()
    {
        var sut = Create();
        Assert.Throws<MediatorException>(() =>
        {
            sut.AddActions(new[] { typeof(object) });
        });
    }

    [Fact]
    public void AddActions_ActionTypePassed_Pass()
    {
        var sut = Create();
        sut.AddActions(new[] { typeof(NopMessage) });
    }

    [Fact]
    public void AddHandlers_NonHandlerTypePassed_ThrowException()
    {
        var sut = Create();
        Assert.Throws<MediatorException>(() =>
        {
            sut.AddHandlers(new[] { typeof(object) });
        });
    }

    [Fact]
    public void AddHandlers_HandlerTypePassed_Pass()
    {
        var sut = Create();
        sut.AddHandlers(new[] { typeof(NopMesageHandler) });
    }

    private MediatorConfigurator Create()
    {
        var sc = new Mock<IServiceCollection>();
        return new MediatorConfigurator(sc.Object);
    }
}