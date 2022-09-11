using System;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization
{
    public abstract class ContractSerializer_CommonTestBase : ContractSerializerBaseTest
    {
        private const string _name = "JSON name";
        private const int _number = 6;
        private static string[] _collection = new string[] { "AAA", "BBB" };
        private static Nested _nested = new Nested { Value = 1.2m };

        protected Func<IContract, bool> Match = c =>
            c.Name == _name &&
            c.Number == _number &&
            c.Nested.Value == _nested.Value &&
            c.Collection[0] == _collection[0] &&
            c.Collection[1] == _collection[1];

        #region Serialize and deserialize Request

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("{}")]
        [InlineData(@"{""Content"":"" "",""Type"":"" ""}")]
        public void Request_InvalidContent_ThrowException(string body)
        {
            var sut = CreateSerializer();
            var ex = Assert.Throws<MediatorHttpException>(() => sut.DeserializeRequest(body));
            Assert.Equal(MediatorHttpException.CreateForInvalidRequest(body).Message, ex.Message);
        }

        [Fact]
        public void Request_PublicPropertyGettersAndSetters_WillPass()
        {
            RunRequestTest(new PublicPropertyGettersAndSettersContract { Name = _name, Number = _number, Collection = _collection, Nested = _nested }, Match);
        }

        [Fact]
        public void Request_ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnly_WillPass()
        {
            RunRequestTest(new ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(_name, _number, _collection, _nested), Match);
        }

        [Fact]
        public void Request_ConstructorWithNotMatchingBindingNamesAndWithPrivateGetter_WillFaill()
        {
            var exception = Assert.Throws<MediatorHttpException>(() =>
            {
                //This is weakness of Microsoft.Text.Json serializer because if there is no parameterless  constructor and public setters, then it deserialize data via names in constructor parameters
                RunRequestTest(new ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(_name, _number, _collection, _nested), Match);
            });
        }

        [Fact]
        public void Request_PublicPropertyGetterAndInitSetter_WillPass()
        {
            RunRequestTest(new PublicPropertyGetterAndInitSetterContract { Name = _name, Number = _number, Collection = _collection, Nested = _nested }, Match);
        }

        [Fact]
        public void Request_PositionalRecord_WillPass()
        {
            RunRequestTest(new PositionalRecordContract(_name, _number, _collection, _nested), Match);
        }

        private void RunRequestTest<TContract>(TContract seed, Func<IContract, bool> match) where TContract : IContract
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
        public void Response_InvalidContent_ThrowException(string body)
        {
            var sut = CreateSerializer();
            var ex = Assert.Throws<MediatorHttpException>(() => sut.DeserializeResponse<Result>(body));
            Assert.Equal(MediatorHttpException.CreateForInvalidResponse(body).Message, ex.Message);
        }

        [Fact]
        public void Response_EmptyObject_ReturnsResponseWithFailureStatus()
        {
            var sut = CreateSerializer();
            var res = sut.DeserializeResponse<Result>(@"{""Content"":"" "",""Type"":"" ""}");
            Assert.False(res.Success);
        }

        [Fact]
        public void Response_PublicPropertyGettersAndSetters_WillPass()
        {
            RunResponseTest(new PublicPropertyGettersAndSettersContract { Name = _name, Number = _number, Collection = _collection, Nested = _nested }, Match);
        }

        [Fact]
        public void Response_ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnly_WillPass()
        {
            RunResponseTest(new ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(_name, _number, _collection, _nested), Match);
        }

        [Fact]
        public void Response_ConstructorWithNotMatchingBindingNamesAndWithPrivateGetter_WillFaill()
        {
            Assert.Throws<MediatorHttpException>(() =>
            {
                //This is weakness of Microsoft.Text.Json serializer because if there is no parameterless  constructor and public setters, then it deserialize data via names in constructor parameters
                RunResponseTest(new ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(_name, _number, _collection, _nested), Match);
            });
        }

        [Fact]
        public void Response_PublicPropertyGetterAndInitSetter_WillPass()
        {
            RunResponseTest(new PublicPropertyGetterAndInitSetterContract { Name = _name, Number = _number, Collection = _collection, Nested = _nested }, Match);
        }

        [Fact]
        public void Response_PositionalRecord_WillPass()
        {
            RunResponseTest(new PositionalRecordContract(_name, _number, _collection, _nested), Match);
        }

        private void RunResponseTest<TDto>(TDto seed, Func<TDto, bool> match)
        {
            var response = new MediatorResponse(true, new object[] { seed! });
            var sut = CreateSerializer();

            var serialized = sut.SerializeResponse(response);
            var deserialized = sut.DeserializeResponse<TDto>(serialized);

            Assert.True(match(deserialized.Result));
        }        

        public interface IResult { }
        public class Result : IResult
        {
            public int Index { get; set; }
        }

        #endregion
               
        #region Actions

        public interface IContract
        {
            public string Name { get; }
            public int Number { get; }
            public string[] Collection { get; }
            public Nested Nested { get; }
        }

        public class PublicPropertyGettersAndSettersContract : IMessage, IContract
        {
            public string Name { get; set; } = "";
            public int Number { get; set; }
            public string[] Collection { get; set; } = new string[0];
            public Nested Nested { get; set; } = new Nested();
        }

        public class ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract : IMessage, IContract
        {
            public ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(string name, int number, string[] collection, Nested nested)
            {
                Name = name;
                Number = number;
                Collection = collection;
                Nested = nested;
            }

            public string Name { get; }
            public int Number { get; }
            public string[] Collection { get; }
            public Nested Nested { get; }
        }

        public class ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract : IMessage, IContract
        {
            public ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(string par1, int par2, string[] collection, Nested nested)
            {
                Name = par1;
                Number = par2;
                Collection = collection;
                Nested = nested;
            }

            public string Name { get; }
            public int Number { get; }
            public string[] Collection { get; }
            public Nested Nested { get; }
        }

        public class PublicPropertyGetterAndInitSetterContract : IMessage, IContract
        {
            public string Name { get; init; } = "";
            public int Number { get; init; }
            public string[] Collection { get; init; } = new string[0];
            public Nested Nested { get; init; } = new();
        }

        public record PositionalRecordContract(string Name, int Number, string[] Collection, Nested Nested) : IMessage, IContract;

        public class Nested
        {
            public decimal Value { get; set; }
        }

        #endregion

    }
}
