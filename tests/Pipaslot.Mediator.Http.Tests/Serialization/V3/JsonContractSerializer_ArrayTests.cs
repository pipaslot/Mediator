using Pipaslot.Mediator.Authorization;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3
{
    public class JsonContractSerializer_ArrayTests : ContractSerializer_ArrayTestBase
    {
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

        protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
        {
            return new JsonContractSerializer(provider);
        }
    }
}
