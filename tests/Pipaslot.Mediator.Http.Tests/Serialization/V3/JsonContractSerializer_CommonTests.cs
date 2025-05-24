using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3;

public class JsonContractSerializer_CommonTests : ContractSerializer_CommonTestBase
{
    [Fact]
    public async Task DeserializeRequest_FromJsonWithShortFormat()
    {
        var sut = CreateSerializer();

        var serialized =
            @"{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializer_CommonTestBase\u002BParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract, Pipaslot.Mediator.Http.Tests"",""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}}";
        var deserialized = await sut.DeserializeRequest(serialized.ConvertToStream(), []);

        Assert.True(Match((ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract)deserialized));
    }

    [Fact]
    public void DeserializeResponse_FromJsonWithShortFormat()
    {
        var sut = CreateSerializer();

        var serialized =
            @"{""Success"":true,""Results"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializer_CommonTestBase\u002BPublicPropertyGetterAndInitSetterContract, Pipaslot.Mediator.Http.Tests"",""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}}],""ErrorMessages"":[]}";
        var deserialized = sut.DeserializeResponse<PublicPropertyGetterAndInitSetterContract>(serialized);

        Assert.True(Match(deserialized.Result));
    }

    public class MessageWithInterfaceProperty : IMessage
    {
        public IContract Contract { get; init; } = null!;
    }

    public class MessageWithInterfaceArrayProperty : IMessage
    {
        public IContract[] Contracts { get; init; } = [];
    }

    public class MessageWithInterfaceCollectionProperty : IMessage
    {
        public List<IContract> Contracts { get; init; } = [];
    }

    public new interface IContract;

    public class Contract : IContract
    {
        public string Name { get; init; } = string.Empty;
    }

    public class ChildMediatorAction : IMessage
    {
        public string Name { get; init; } = string.Empty;
    }

    protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
    {
        var optionsMock = new Mock<IMediatorOptions>();
        return new JsonContractSerializer(provider, optionsMock.Object);
    }
}