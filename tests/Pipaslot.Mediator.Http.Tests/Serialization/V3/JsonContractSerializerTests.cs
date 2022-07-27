using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3
{
    public class JsonContractSerializerTests : ContractSerializerTestBase
    {
        [Fact]
        public void SerializeRequest_InterfaceCollectionProperty_SerializeAsSpecificType()
        {
            var expected = @"{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializerTests\u002BMessageWithInterfaceCollectionProperty, Pipaslot.Mediator.Http.Tests"",""Contracts"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializerTests\u002BContract, Pipaslot.Mediator.Http.Tests"",""Name"":""Contract name""}]}";
            var contract = new Contract
            {
                Name = "Contract name"
            };
            var action = new MessageWithInterfaceCollectionProperty
            {
                Contracts = new IContract[] { contract }
            };
            var sut = CreateSerializer();
            var serialized = sut.SerializeRequest(action);

            Assert.Equal(expected, serialized);
        }

        [Fact]
        public void SerializeResponse_InterfaceCollection_SerializeAsSpecificType()
        {
            var expected = @"{""Success"":true,""Results"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializerTests\u002BIContract[], Pipaslot.Mediator.Http.Tests"",""Items"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializerTests\u002BContract, Pipaslot.Mediator.Http.Tests"",""Name"":""Contract name""}]}]}";
            var contract = new Contract
            {
                Name = "Contract name"
            };
            var response = new MediatorResponse(true, new object[] { new IContract[] { contract } });
            var sut = CreateSerializer();

            var serialized = sut.SerializeResponse(response);

            Assert.Equal(expected, serialized);
        }

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
        public void Request_SerializePropertyInterfaceCollection()
        {
            var contract = new Contract
            {
                Name = "Contract name"
            };
            var action = new MessageWithInterfaceCollectionProperty
            {
                Contracts = new IContract[] { contract }
            };
            var sut = CreateSerializer();

            var serialized = sut.SerializeRequest(action);
            var deserialized = (MessageWithInterfaceCollectionProperty)sut.DeserializeRequest(serialized);
            Assert.NotNull(deserialized);
            Assert.Equal(typeof(IContract[]), deserialized.Contracts.GetType());
            Assert.Equal(((Contract)deserialized.Contracts.First()).Name, contract.Name);
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
            var response = new MediatorResponse(true, new object[] { action });
            var sut = CreateSerializer();

            var serialized = sut.SerializeResponse(response);
            var deserialized = sut.DeserializeResponse<MessageWithInterfaceProperty>(serialized);

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.Result);
            Assert.Equal(deserialized.Result.Contract.GetType(), contract.GetType());
            Assert.Equal(((Contract)deserialized.Result.Contract).Name, contract.Name);
        }

        [Fact]
        public void Response_SerializePropertyCollectionInterface()
        {
            var contract = new Contract
            {
                Name = "Contract name"
            };
            var action = new MessageWithInterfaceCollectionProperty
            {
                Contracts = new IContract[] { contract }
            };
            var response = new MediatorResponse(true, new object[] { action });
            var sut = CreateSerializer();

            var serialized = sut.SerializeResponse(response);
            var deserialized = sut.DeserializeResponse<MessageWithInterfaceCollectionProperty>(serialized);

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.Result);
            Assert.Equal(typeof(IContract[]), deserialized.Result.Contracts.GetType());
            Assert.Equal(((Contract)deserialized.Result.Contracts.First()).Name, contract.Name);
        }

        [Fact]
        public void Response_SerializeInterfaceCollection()
        {
            var contract = new Contract
            {
                Name = "Contract name"
            };
            var response = new MediatorResponse(true, new object[] { new IContract[] { contract } });
            var sut = CreateSerializer();

            var serialized = sut.SerializeResponse(response);
            var deserialized = sut.DeserializeResponse<IContract[]>(serialized);

            Assert.NotNull(deserialized);
            Assert.NotNull(deserialized.Result);
            Assert.Equal(typeof(IContract[]), deserialized.Result.GetType());
            Assert.Equal(((Contract)deserialized.Result.First()).Name, contract.Name);
        }

        [Fact]
        public void Request_InterfaceCollection_ShouldCallVerifyCredibility()
        {
            var contract = new Contract();
            var action = new MessageWithInterfaceCollectionProperty
            {
                Contracts = new IContract[] { contract }
            };
            CredibleProviderMock = new Mock<ICredibleProvider>(MockBehavior.Strict);
            CredibleProviderMock
                .Setup(p => p.VerifyCredibility(action.GetType()));
            CredibleProviderMock
                .Setup(p => p.VerifyCredibility(contract.GetType()));

            var sut = CreateSerializer();
            var serialized = sut.SerializeRequest(action);
            sut.DeserializeRequest(serialized);

            CredibleProviderMock.VerifyAll();
        }

        [Fact]
        public void Request_InterfaceProperty_ShouldCallVerifyCredibility()
        {
            var contract = new Contract();
            var action = new MessageWithInterfaceProperty
            {
                Contract = contract
            };
            VerifyRequestCredibility(action, action.GetType(), contract.GetType());
        }

        [Fact]
        public void Response_InterfaceProperty_ShouldCallVerifyCredibility()
        {
            var contract = new Contract();
            var result = new MessageWithInterfaceProperty
            {
                Contract = contract
            };
            VerifyResponseCredibility(result, result.GetType(), contract.GetType());
        }

        [Fact]
        public void Response_InterfaceCollection_ShouldCallVerifyCredibility()
        {
            var contract = new Contract();
            var result = new IContract[]
            {
                contract
            };
            VerifyResponseCredibility(result, contract.GetType());
        }

        public class MessageWithInterfaceProperty : IMessage
        {
            public IContract Contract { get; set; } = null!;
        }

        public class MessageWithInterfaceCollectionProperty : IMessage
        {
            public IContract[] Contracts { get; set; } = Array.Empty<IContract>();
        }

        public interface IContract
        {

        }
        public class Contract : IContract
        {
            public string Name { get; set; } = string.Empty;
        }
        public class MessageWithIMediatorActionProperty : IMessage
        {
            public IMediatorAction SubAction { get; set; } = null!;
        }
        public class ChildMediatorAction : IMessage
        {
            public string Name { get; set; } = string.Empty;
        }

        protected override IContractSerializer CreateSerializer()
        {
            return new JsonContractSerializer(CredibleProviderMock.Object);
        }
    }
}
