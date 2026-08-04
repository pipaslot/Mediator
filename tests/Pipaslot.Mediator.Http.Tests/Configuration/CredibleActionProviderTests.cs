using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http.Configuration;
using System;

namespace Pipaslot.Mediator.Http.Tests.Configuration;

public class CredibleActionProviderTests
{
    [Fact]
    public void VerifyCredibility_ContractTypeIsNotFromRegisteredAssembly_ThrowException()
    {
        var sut = Create(c => c.AddActionsFromAssemblyOf<IMediator>());
        var exception = Assert.Throws<MediatorHttpException>(() => sut.VerifyCredibility(typeof(FakeContract)));
        Assert.Contains(typeof(FakeContract).FullName!, exception.Message);
    }

    [Fact]
    public void VerifyCredibility_RegisteredContractTypeNotImplementingIActionMarkerInterface_ThrowException()
    {
        var sut = Create(c => c.AddActionsFromAssemblyOf<FakeNonContract>());
        var exception = Assert.Throws<MediatorHttpException>(() => sut.VerifyCredibility(typeof(FakeNonContract)));
        Assert.Contains(typeof(FakeNonContract).FullName!, exception.Message);
    }

    [Fact]
    public void VerifyCredibility_ContractTypeIsFromRegisteredAssembly_Pass()
    {
        var sut = Create(c => c.AddActionsFromAssemblyOf<FakeContract>());
        sut.VerifyCredibility(typeof(FakeContract));
    }

    private CredibleActionProvider Create(Action<MediatorConfigurator> setup, params Type[] customTypes)
    {
        var serviceCollection = Substitute.For<IServiceCollection>();
        var configurator = new MediatorConfigurator(serviceCollection);
        setup(configurator);
        return new CredibleActionProvider(configurator, customTypes, []);
    }

    public class FakeContract : IMessage;

    public class FakeNonContract;
}