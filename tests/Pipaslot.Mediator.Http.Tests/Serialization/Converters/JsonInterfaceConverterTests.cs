using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.Converters;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.Converters
{
    public class JsonInterfaceConverterTests 
    {
        [Fact]
        public void Request_ActionPropertyIsInterface_ShouldSerialize()
        {
            var sut = CreateSerializer();
            var action = new ActionWithInterfaceProperty
            {
                Data = new CustomDate
                {
                    Name = "CustomResultName",
                    AnotherData = "AnotherData"
                }
            };
            var serialized = sut.SerializeRequest(action);
            var deserialized = (ActionWithInterfaceProperty)sut.DeserializeRequest(serialized);

            Assert.Equal(action.Data.GetType(), deserialized.Data.GetType());
            Assert.Equal(action.Data.Name, deserialized.Data.Name);
            Assert.Equal(((CustomDate)action.Data).AnotherData, ((CustomDate)deserialized.Data).AnotherData);
        }

        [Fact]
        public void Response_ActionPropertyIsInterface_ShouldSerialize()
        {
            var sut = CreateSerializer();
            var action = new ActionWithInterfaceProperty
            {
                Data = new CustomDate
                {
                    Name = "CustomResultName",
                    AnotherData = "AnotherData"
                }
            };
            var response = new MediatorResponse(true, new object[] { action }, new string[0]);
            var serialized = sut.SerializeResponse(response);
            var deserialized = sut.DeserializeResponse<ActionWithInterfaceProperty>(serialized);

            Assert.Equal(action.Data.GetType(), deserialized.Result.Data.GetType());
            Assert.Equal(action.Data.Name, deserialized.Result.Data.Name);
            Assert.Equal(((CustomDate)action.Data).AnotherData, ((CustomDate)deserialized.Result.Data).AnotherData);
        }

        private IContractSerializer CreateSerializer()
        {
            return new FullJsonContractSerializer(new Mock<ICredibleActionProvider>().Object, new Mock<ICredibleResultProvider>().Object);
        }

        public class ActionWithInterfaceProperty : IMessage
        {
            public IData Data { get; set; }
        }

        [JsonInterfaceConverter(typeof(IData))]
        public interface IData
        {
            public string Name { get; set; }
        }

        public class CustomDate : IData
        {
            public string Name { get; set; }
            public string AnotherData { get; set; }
        }
    }
}
