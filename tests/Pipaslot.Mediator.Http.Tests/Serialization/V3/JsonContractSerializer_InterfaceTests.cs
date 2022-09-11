using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System.Linq;
using Xunit;
using static Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializer_CommonTests;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3
{
    public class JsonContractSerializer_InterfaceTests : ContractSerializer_InterfaceTestBase
    {
        protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
        {
            return new JsonContractSerializer(provider);
        }

        [Fact]
        public void SerializeRequest_InterfaceCollectionProperty_SerializeAsSpecificType()
        {
            var expected = @"{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializer_CommonTests\u002BMessageWithInterfaceCollectionProperty, Pipaslot.Mediator.Http.Tests"",""Contracts"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializer_CommonTests\u002BContract, Pipaslot.Mediator.Http.Tests"",""Name"":""Contract name""}]}";
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
            var expected = @"{""Success"":true,""Results"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializer_CommonTests\u002BIContract[], Pipaslot.Mediator.Http.Tests"",""Items"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializer_CommonTests\u002BContract, Pipaslot.Mediator.Http.Tests"",""Name"":""Contract name""}]}]}";
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
    }
}
