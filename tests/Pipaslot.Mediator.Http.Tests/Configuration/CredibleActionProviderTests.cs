using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Configuration;
using System;

namespace Pipaslot.Mediator.Http.Tests.Configuration;
public class CredibleActionProviderTests
{
    [Test]
    public void VerifyCredibility_ContractTypeIsNotFromRegisteredAssembly_ThrowException()
    {
        var sut = Create(c => c.AddActionsFromAssemblyOf<IMediator>());
        var exception = Assert.Throws<MediatorHttpException>(() => sut.VerifyCredibility(typeof(FakeContract)));
        Assert.Equal(MediatorHttpException.CreateForUnregisteredActionType(typeof(FakeContract)).Message, exception.Message);
    }

    [Test]
    public void VerifyCredibility_RegisteredContractTypeNotImplementingIActionMarkerInterface_ThrowException()
    {
        var sut = Create(c => c.AddActionsFromAssemblyOf<FakeNonContract>());
        var exception = Assert.Throws<MediatorHttpException>(() => sut.VerifyCredibility(typeof(FakeNonContract)));
        Assert.Equal(MediatorHttpException.CreateForNonContractType(typeof(FakeNonContract)).Message, exception.Message);
    }

    [Test]
    public void VerifyCredibility_ContractTypeIsFromRegisteredAssembly_Pass()
    {
        var sut = Create(c => c.AddActionsFromAssemblyOf<FakeContract>());
        sut.VerifyCredibility(typeof(FakeContract));
    }

    private CredibleActionProvider Create(Action<MediatorConfigurator> setup, params Type[] customTypes)
    {
        var serviceCollectionMock = new Mock<IServiceCollection>();
        var configurator = new MediatorConfigurator(serviceCollectionMock.Object);
        setup(configurator);
        return new CredibleActionProvider(configurator, customTypes, []);
    }

    public class FakeContract : Pipaslot.Mediator.IMessage, Pipaslot.Mediator.Abstractions.IMediatorAction;
    public class FakeNonContract;
}