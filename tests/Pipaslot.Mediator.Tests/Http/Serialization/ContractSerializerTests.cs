using Pipaslot.Mediator.Http.Serialization;

namespace Pipaslot.Mediator.Tests.Http.Serialization
{
    public class ContractSerializerTests : ContractSerializerTestBase
    {
        protected override IContractSerializer CreateSerializer()
        {
            return new ContractSerializer(ActionProviderMock.Object);
        }
    }
}
