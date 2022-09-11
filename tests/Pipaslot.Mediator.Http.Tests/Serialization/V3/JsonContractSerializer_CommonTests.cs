using Moq;
using Pipaslot.Mediator.Abstractions;
using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3
{
    public class JsonContractSerializer_CommonTests : ContractSerializer_CommonTestBase
    {      

        [Fact]
        public void DeserializeRequest_FromJsonWithShortFormat()
        {
            var sut = CreateSerializer();

            var serialized = @"{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializer_CommonTestBase\u002BParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract, Pipaslot.Mediator.Http.Tests"",""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}}";
            var deserialized = sut.DeserializeRequest(serialized);

            Assert.True(Match((ParametricConstructorWithMatchingNamesAndPublicPropertyGetterOnlyContract)deserialized));
        }

        [Fact]
        public void DeserializeResponse_FromJsonWithShortFormat()
        {
            var sut = CreateSerializer();

            var serialized = @"{""Success"":true,""Results"":[{""$type"":""Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializer_CommonTestBase\u002BPublicPropertyGetterAndInitSetterContract, Pipaslot.Mediator.Http.Tests"",""Name"":""JSON name"",""Number"":6,""Collection"":[""AAA"",""BBB""],""Nested"":{""Value"":1.2}}],""ErrorMessages"":[]}";
            var deserialized = sut.DeserializeResponse<PublicPropertyGetterAndInitSetterContract>(serialized);

            Assert.True(Match(deserialized.Result));
        }

        

        [Fact]
        public void Response_ResultTypeWithCollectionOfInterfaceInheritingCollection_WillKeepTheResultType()
        {
            var sut = CreateSerializer();
            var result = new IsAuthorizedRequestResponse(new IRuleSet[] {
                new RuleSet
                {
                    new Rule("name", "value", false)
                }
            });
            var responseString = sut.SerializeResponse(new MediatorResponse(true, new object[] { result }));
            var deserialized = sut.DeserializeResponse<IsAuthorizedRequestResponse>(responseString);
            Assert.Equal(result.GetType(), deserialized.Result.GetType());
        }

        [Fact]
        public void Response_ResultTypeWithCollectionOfObjectInheritingCollection_WillKeepTheResultType()
        {
            var sut = CreateSerializer();
            var result = new FakeRuleSetWithSpecificArrayResponse
            {
                NotMetRule = new RuleSet[] {
                    new RuleSet
                    {
                        new Rule("name", "value", false)
                    }
                }
            };
            var responseString = sut.SerializeResponse(new MediatorResponse(true, new object[] { result }));
            var deserialized = sut.DeserializeResponse<FakeRuleSetWithSpecificArrayResponse>(responseString);
            Assert.Equal(result.GetType(), deserialized.Result.GetType());
        }
        public class FakeRuleSetWithSpecificArrayResponse
        {
            public bool IsAuthorized => NotMetRule.Count() == 0;
            public RuleSet[] NotMetRule { get; set; } = Array.Empty<RuleSet>();
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

        protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
        {
            return new JsonContractSerializer(provider);
        }
    }
}
