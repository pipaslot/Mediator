﻿using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V2;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V2;

public class FullJsonContractSerializer_CommonTests : ContractSerializer_CommonTestBase
{
    [Fact]
    public void DeserializeRequest_FromJson()
    {
        var sut = CreateSerializer();

        var serialized =
            @"{""Content"":{""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}},""Type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializer_CommonTestBase\u002BParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract, Pipaslot.Mediator.Http.Tests""}";
        var deserialized = sut.DeserializeRequest(serialized);

        Assert.True(Match((ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract)deserialized));
    }

    [Fact]
    public void DeserializeResponse_FromJson()
    {
        var sut = CreateSerializer();

        var serialized =
            @"{""Success"":true,""Results"":[{""Content"":{""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}},""Type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializer_CommonTestBase\u002BPublicPropertyGetterAndInitSetterContract, Pipaslot.Mediator.Http.Tests""}],""ErrorMessages"":[]}";
        var deserialized = sut.DeserializeResponse<PublicPropertyGetterAndInitSetterContract>(serialized);

        Assert.True(Match(deserialized.Result));
    }

    protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
    {
        return new FullJsonContractSerializer(provider);
    }
}