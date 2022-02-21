using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization
{
    public abstract class ContractSerializerTestBase
    {
        private const string _name = "JSON name";
        private const int _number = 6;

        protected Mock<ICredibleActionProvider> ActionProviderMock = new();
        protected Mock<ICredibleResultProvider> ResultProviderMock = new();

        #region Serialize and deserialize Request
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("{}")]
        [InlineData(@"{""Content"":"" "",""Type"":"" ""}")]
        public void DeserializeRequest_InvalidContent_ThrowException(string body)
        {
            var sut = CreateSerializer();
            var ex = Assert.Throws<MediatorHttpException>(() => sut.DeserializeRequest(body));
            Assert.Equal(MediatorHttpException.CreateForInvalidRequest(body).Message, ex.Message);
        }

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
            var exception = Assert.Throws<MediatorHttpException>(() =>
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

        #region Serialize and Deserialize Response

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void DeserializeResponse_InvalidContent_ThrowException(string body)
        {
            var sut = CreateSerializer();
            var ex = Assert.Throws<MediatorHttpException>(() => sut.DeserializeResponse<Result>(body));
            Assert.Equal(MediatorHttpException.CreateForInvalidResponse(body).Message, ex.Message);
        }

        [Fact]
        public void DeserializeResponse_EmptyObject_ReturnsResponseWithFailureStatus()
        {
            var sut = CreateSerializer();
            var res = sut.DeserializeResponse<Result>(@"{""Content"":"" "",""Type"":"" ""}");
            Assert.False(res.Success);
        }

        [Fact]
        public void DeserializeResponse_PublicPropertyGettersAndSetters_WillPass()
        {
            RunResponseTest(new PublicPropertyGettersAndSettersContract { Name = _name, Number = _number }, c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void DeserializeResponse_ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnly_WillPass()
        {
            RunResponseTest(new ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(_name, _number), c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void DeserializeResponse_ConstructorWithNotMatchingBindingNamesAndWithPrivateGetter_WillFaill()
        {
            Assert.Throws<MediatorHttpException>(() =>
            {
                //This is weakness of Microsoft.Text.Json serializer because if there is no parameterless  constructor and public setters, then it deserialize data via names in constructor parameters
                RunResponseTest(new ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(_name, _number), c => c.Name == _name && c.Number == _number);
            });
        }

        [Fact]
        public void DeserializeResponse_PublicPropertyGetterAndInitSetter_WillPass()
        {
            RunResponseTest(new PublicPropertyGetterAndInitSetterContract { Name = _name, Number = _number }, c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void DeserializeResponse_PositionalRecord_WillPass()
        {
            RunResponseTest(new PositionalRecordContract(_name, _number), c => c.Name == _name && c.Number == _number);
        }

        private void RunResponseTest<TDto>(TDto seed, Func<TDto, bool> match)
        {
            var results = new System.Collections.Generic.List<object>
            {
                seed!
            };
            var response = new MediatorResponse(true, results, Array.Empty<string>());
            var sut = CreateSerializer();

            var serialized = sut.SerializeResponse(response);
            var deserialized = sut.DeserializeResponse<TDto>(serialized);

            Assert.True(match(deserialized.Result));
        }

        [Fact]
        public void DeserializeResponse_ResultTypeIsInterface_WillKeepTheResultType()
        {
            var sut = CreateSerializer();
            var result = new Result();
            var responseString = sut.SerializeResponse(new MediatorResponse(true, new object[] { result }, Array.Empty<string>()));
            var deserialized = sut.DeserializeResponse<IResult>(responseString);
            Assert.Equal(result.GetType(), deserialized.Result.GetType());
        }

        public interface IResult { }
        public class Result : IResult { }

        #endregion


        protected abstract IContractSerializer CreateSerializer();

        #region Actions

        public class PublicPropertyGettersAndSettersContract : IMessage
        {
            public string Name { get; set; } = "";
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
            public string Name { get; init; } = "";
            public int Number { get; init; }
        }

        public record PositionalRecordContract(string Name, int Number) : IMessage;

        #endregion

    }
}
