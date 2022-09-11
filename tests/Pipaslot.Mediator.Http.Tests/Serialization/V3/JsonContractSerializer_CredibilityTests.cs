using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using static Pipaslot.Mediator.Http.Tests.Serialization.V3.JsonContractSerializerTests;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3
{
    public class JsonContractSerializer_CredibilityTests : ContractSerializer_CredibilityTestBase
    {
        protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
        {
            return new JsonContractSerializer(provider);
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
    }
}
