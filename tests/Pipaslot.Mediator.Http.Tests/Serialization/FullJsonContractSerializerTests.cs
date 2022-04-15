using Pipaslot.Mediator.Http.Serialization;

namespace Pipaslot.Mediator.Http.Tests.Serialization
{
    public class FullJsonContractSerializerTests : ContractSerializerTestBase
    {
        protected override IContractSerializer CreateSerializer()
        {
            return new FullJsonContractSerializer(ActionProviderMock.Object, ResultProviderMock.Object);
        }
    }
}
