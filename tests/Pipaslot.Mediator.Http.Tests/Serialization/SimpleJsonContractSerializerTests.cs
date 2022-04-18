using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Serialization;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization
{
    public class SimpleJsonContractSerializerTests : ContractSerializerTestBase
    {
        [Fact]
        public void DeserializeRequest_FromJsonWithShortFormat()
        {
            var sut = CreateSerializer();

            var serialized = @"{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializerTestBase\u002BParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract, Pipaslot.Mediator.Http.Tests"",""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}}";
            var deserialized = sut.DeserializeRequest(serialized);

            Assert.True(Match((ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract)deserialized));
        }

        [Fact]
        public void DeserializeResponse_FromJsonWithShortFormat()
        {
            var sut = CreateSerializer();

            var serialized = @"{""Success"":true,""Results"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializerTestBase\u002BPublicPropertyGetterAndInitSetterContract, Pipaslot.Mediator.Http.Tests"",""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}}],""ErrorMessages"":[]}";
            var deserialized = sut.DeserializeResponse<PublicPropertyGetterAndInitSetterContract>(serialized);

            Assert.True(Match(deserialized.Result));
        }

        [Fact]
        public void Request_SerializePropertyInterface()
        {
            var contract = new Contract
            {
                Name = "Contract name"
            };
            var action = new MessageWithInterfaceProperty
            {
                Contract = contract
            };
            var sut = CreateSerializer();

            var serialized = sut.SerializeRequest(action);
            var deserialized = (MessageWithInterfaceProperty)sut.DeserializeRequest(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(deserialized.Contract.GetType(), contract.GetType());
            Assert.Equal(((Contract)deserialized.Contract).Name, contract.Name);
        }

        [Fact]
        public void Request_SerializePropertyIMediatorction()
        {
            var subAction = new ChildMediatorAction
            {
                Name = "Contract name"
            };
            var action = new MessageWithIMediatorActionProperty
            {
                SubAction = subAction
            };
            var sut = CreateSerializer();

            var serialized = sut.SerializeRequest(action);
            var deserialized = (MessageWithIMediatorActionProperty)sut.DeserializeRequest(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(deserialized.SubAction.GetType(), subAction.GetType());
            Assert.Equal(((ChildMediatorAction)deserialized.SubAction).Name, subAction.Name);
        }

        [Fact]
        public void Response_SerializePropertyInterface()
        {
            var contract = new Contract
            {
                Name = "Contract name"
            };
            var action = new MessageWithInterfaceProperty
            {
                Contract = contract
            };
            var response = new MediatorResponse(true, new object[] { action }, Array.Empty<string>());
            var sut = CreateSerializer();

            var serialized = sut.SerializeResponse(response);
            var deserialized = sut.DeserializeResponse<MessageWithInterfaceProperty>(serialized);

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.Result);
            Assert.Equal(deserialized.Result.Contract.GetType(), contract.GetType());
            Assert.Equal(((Contract)deserialized.Result.Contract).Name, contract.Name);
        }

        public class MessageWithInterfaceProperty : IMessage
        {
            public IContract Contract { get; set; }
        }

        public interface IContract
        {

        }
        public class Contract : IContract
        {
            public string Name { get; set; }
        }
        public class MessageWithIMediatorActionProperty : IMessage
        {
            public IMediatorAction SubAction { get; set; }
        }
        public class ChildMediatorAction : IMessage
        {
            public string Name { get; set; }
        }

        protected override IContractSerializer CreateSerializer()
        {
            return new SimpleJsonContractSerializer(ActionProviderMock.Object, ResultProviderMock.Object);
        }
    }
}
