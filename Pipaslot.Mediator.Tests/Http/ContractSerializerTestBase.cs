using Microsoft.Extensions.DependencyInjection;
using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Tests.Http
{
    public abstract class ContractSerializerTestBase
    {
        private const string _name = "JSON name";
        private const int _number = 6;

        #region Request serialization

        [Fact]
        public void Request_PublicPropertyGettersAndSetters_WillPass()
        {
            RunRequestTest(new PublicPropertyGettersAndSettersContract { Name = _name, Number = _number }, c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void Request_ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnly_WillPass()
        {
            RunRequestTest(new ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(_name, _number), c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void Request_ConstructorWithNotMatchingBindingNamesAndWithPrivateGetter_WillFaill()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                //This is weakness of Microsoft.Text.Json serializer because if there is no parameterless  constructor and public setters, then it deserialize data via names in constructor parameters
                RunRequestTest(new ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(_name, _number), c => c.Name == _name && c.Number == _number);
            });
        }

        [Fact]
        public void Request_PublicPropertyGetterAndInitSetter_WillPass()
        {
            RunRequestTest(new PublicPropertyGetterAndInitSetterContract { Name = _name, Number = _number }, c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void Request_PositionalRecord_WillPass()
        {
            RunRequestTest(new PositionalRecordContract(_name, _number), c => c.Name == _name && c.Number == _number);
        }

        private void RunRequestTest<TContract>(TContract seed, Func<TContract, bool> match) where TContract : IMediatorAction
        {
            var sut = CreateSerializer();

            var serialized = sut.SerializeRequest(seed);
            var deserialized = sut.DeserializeRequest(serialized);

            Assert.True(match((TContract)deserialized));
        }

        #endregion

        #region Response serialization

        [Fact]
        public void Response_PublicPropertyGettersAndSetters_WillPass()
        {
            RunResponseTest(new PublicPropertyGettersAndSettersContract { Name = _name, Number = _number }, c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void Response_ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnly_WillPass()
        {
            RunResponseTest(new ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(_name, _number), c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void Response_ConstructorWithNotMatchingBindingNamesAndWithPrivateGetter_WillFaill()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                //This is weakness of Microsoft.Text.Json serializer because if there is no parameterless  constructor and public setters, then it deserialize data via names in constructor parameters
                RunResponseTest(new ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(_name, _number), c => c.Name == _name && c.Number == _number);
            });
        }

        [Fact]
        public void Response_PublicPropertyGetterAndInitSetter_WillPass()
        {
            RunResponseTest(new PublicPropertyGetterAndInitSetterContract { Name = _name, Number = _number }, c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void Response_PositionalRecord_WillPass()
        {
            RunResponseTest(new PositionalRecordContract(_name, _number), c => c.Name == _name && c.Number == _number);
        }

        private void RunResponseTest<TDto>(TDto seed, Func<TDto, bool> match)
        {
            var results = new System.Collections.Generic.List<object>
            {
                seed
            };
            var response = new MediatorResponse(true, results, new string[0]);
            var sut = CreateSerializer();

            var serialized = sut.SerializeResponse(response);
            var deserialized = sut.DeserializeResponse<TDto>(serialized);

            Assert.True(match(deserialized.Result));
        }


        #endregion

        #region Request deserialization validation

        [Fact]
        public void DeserializeRequest_ContractTypeIsNotFromRegisteredAssembly_ThrowException()
        {
            var sut = CreateSerializer(c => c.AddActionsFromAssemblyOf<Mediator>());
            var request = sut.SerializeRequest(new FakeContract());
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                sut.DeserializeRequest(request);
            });
            Assert.Equal(MediatorHttpException.CreateForUnregisteredType(typeof(FakeContract)).Message, exception.Message);
        }

        [Fact]
        public void DeserializeRequest_RegisteredContractTypeNotImplementingIActionMarkerInterface_ThrowException()
        {
            var sut = CreateSerializer(c => c.AddActionsFromAssemblyOf<FakeNonContract>());
            var request = sut.SerializeRequest(new FakeNonContract());
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                sut.DeserializeRequest(request);
            });
            Assert.Equal(MediatorHttpException.CreateForNonContractType(typeof(FakeNonContract)).Message, exception.Message);
        }

        [Fact]
        public void DeserializeRequest_ContractTypeIsFromRegisteredAssembly_Pass()
        {
            var sut = CreateSerializer(c => c.AddActionsFromAssemblyOf<FakeContract>());
            var request = sut.SerializeRequest(new FakeContract());
            var deserialized = sut.DeserializeRequest(request);
            Assert.NotNull(deserialized);
        }

        public class FakeContract : IMessage
        {

        }

        public class FakeNonContract
        {

        }

        #endregion

        #region Response deserialization

        public void DeserializeResponse_ResultTypeIsInterface_WillKeepTheResultType()
        {
            var sut = CreateSerializer(c => c.AddActionsFromAssemblyOf<FakeContract>());
            var result = new Result();
            var responseString = sut.SerializeResponse(new MediatorResponse(true, new object[] {result}, new string[0]));
            var deserialized = sut.DeserializeResponse<IResult>(responseString);
            Assert.Equal(result.GetType(), deserialized.Result.GetType());
        }

        public interface IResult { }
        public class Result : IResult { }

        #endregion

        private IContractSerializer CreateSerializer()
        {
            return CreateSerializer(c => c.AddActionsFromAssemblyOf<FakeContract>());
        }

        private IContractSerializer CreateSerializer(Action<PipelineConfigurator> setup)
        {
            var serviceCollctionMock = new Mock<IServiceCollection>();
            var configurator = new PipelineConfigurator(serviceCollctionMock.Object);
            setup(configurator);
            return CreateSerializer(configurator);
        }

        protected abstract IContractSerializer CreateSerializer(PipelineConfigurator configurator);

        #region Actions

        public class PublicPropertyGettersAndSettersContract : IMessage
        {
            public string Name { get; set; }
            public int Number { get; set; }
        }

        public class ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract : IMessage
        {
            public ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(string name, int number)
            {
                Name = name;
                Number = number;
            }

            public string Name { get; }
            public int Number { get; }
        }

        public class ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract : IMessage
        {
            public ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(string par1, int par2)
            {
                Name = par1;
                Number = par2;
            }

            public string Name { get; }
            public int Number { get; }
        }

        public class PublicPropertyGetterAndInitSetterContract : IMessage
        {
            public string Name { get; init; }
            public int Number { get; init; }
        }

        public record PositionalRecordContract(string Name, int Number) : IMessage;

        #endregion

    }
}
