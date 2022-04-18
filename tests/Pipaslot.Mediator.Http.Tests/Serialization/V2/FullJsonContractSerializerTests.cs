using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V2;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V2
{
    public class FullJsonContractSerializerTests : ContractSerializerTestBase
    {
        [Fact]
        public void DeserializeRequest_FromJson()
        {
            var sut = CreateSerializer();

            var serialized = @"{""Content"":{""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}},""Type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializerTestBase\u002BParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract, Pipaslot.Mediator.Http.Tests""}";
            var deserialized = sut.DeserializeRequest(serialized);

            Assert.True(Match((ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract)deserialized));
        }

        [Fact]
        public void DeserializeResponse_FromJson()
        {
            var sut = CreateSerializer();

            var serialized = @"{""Success"":true,""Results"":[{""Content"":{""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}},""Type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializerTestBase\u002BPublicPropertyGetterAndInitSetterContract, Pipaslot.Mediator.Http.Tests""}],""ErrorMessages"":[]}";
            var deserialized = sut.DeserializeResponse<PublicPropertyGetterAndInitSetterContract>(serialized);

            Assert.True(Match(deserialized.Result));
        }

        protected override IContractSerializer CreateSerializer()
        {
            return new FullJsonContractSerializer(CredibleProviderMock.Object);
        }
    }
}
