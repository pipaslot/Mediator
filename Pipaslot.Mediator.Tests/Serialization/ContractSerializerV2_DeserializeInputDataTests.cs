using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Serialization;
using System;
using Xunit;

namespace Pipaslot.Mediator.Tests.Serialization
{
    public class ContractSerializerV2_DeserializeInputDataTests
    {
        private const string _name = "JSON name";
        private const int _number = 6;

        [Fact]
        public void  PublicPropertyGettersAndSetters_WillPass()
        {
            RunTest(new PublicPropertyGettersAndSettersContract { Name = _name, Number = _number }, c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void  ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnly_WillPass()
        {
            RunTest(new ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract(_name, _number), c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void  ConstructorWithNotMatchingBindingNamesAndWithPrivateGetter_WillFaill()
        {
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                //This is weakness of Microsoft.Text.Json serializer because if there is no parameterless  constructor and public setters, then it deserialize data via names in constructor parameters
                RunTest(new ConstructorWithNotMatchingBindingNamesAndWithPrivateGetterContract(_name, _number), c => c.Name == _name && c.Number == _number);
            });
        }

        [Fact]
        public void  PublicPropertyGetterAndInitSetter_WillPass()
        {
            RunTest(new PublicPropertyGetterAndInitSetterContract { Name = _name, Number = _number }, c => c.Name == _name && c.Number == _number);
        }

        [Fact]
        public void  PositionalRecord_WillPass()
        {
            RunTest(new PositionalRecordContract(_name, _number), c => c.Name == _name && c.Number == _number);
        }

        private static void RunTest<TContract>(TContract seed, Func<TContract, bool> match) where TContract : IMediatorAction
        {
            var sut = new ContractSerializer();

            var serialized = sut.SerializeRequest(seed, out var _);
            var deserialized = sut.DeserializeRequest(serialized);

            Assert.True(match((TContract)deserialized.Content));
        }

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

    }
}
