using Pipaslot.Mediator.Http.Serialization;

namespace Pipaslot.Mediator.Http.Tests.Serialization
{
    public class ContractSerializerTests : ContractSerializerTestBase
    {
        protected override IContractSerializer CreateSerializer()
        {
            return new ContractSerializer(ActionProviderMock.Object);
        }
    }
}
