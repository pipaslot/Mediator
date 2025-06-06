﻿using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using Xunit;
using static Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializer_CommonTests;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3;

public class JsonContractSerializer_CredibilityTests : ContractSerializer_CredibilityTestBase
{
    protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
    {
        var optionsMock = new Mock<IMediatorOptions>();
        return new JsonContractSerializer(provider, optionsMock.Object);
    }

    [Fact]
    public void Request_InterfaceProperty_ShouldCallVerifyCredibility()
    {
        var contract = new Contract();
        var action = new MessageWithInterfaceProperty { Contract = contract };
        VerifyRequestCredibility(action, action.GetType(), contract.GetType());
    }

    [Fact]
    public void Response_InterfaceProperty_ShouldCallVerifyCredibility()
    {
        var contract = new Contract();
        var result = new MessageWithInterfaceProperty { Contract = contract };
        VerifyResponseCredibility(result, result.GetType(), contract.GetType());
    }

    [Fact]
    public void Response_InterfaceCollection_ShouldCallVerifyCredibility()
    {
        var contract = new Contract();
        var result = new IContract[] { contract };
        VerifyResponseCredibility(result, contract.GetType());
    }

    [Fact]
    public void Request_InterfaceCollection_ShouldCallVerifyCredibility()
    {
        var contract = new Contract();
        var action = new MessageWithInterfaceArrayProperty { Contracts = [contract] };
        CredibleProviderMock = new Mock<ICredibleProvider>(MockBehavior.Strict);
        CredibleProviderMock
            .Setup(p => p.VerifyCredibility(action.GetType()));
        CredibleProviderMock
            .Setup(p => p.VerifyCredibility(contract.GetType()));

        var sut = CreateSerializer();
        var serialized = sut.SerializeRequest(action);
        sut.DeserializeRequest(serialized.Json, serialized.Streams);

        CredibleProviderMock.VerifyAll();
    }
}