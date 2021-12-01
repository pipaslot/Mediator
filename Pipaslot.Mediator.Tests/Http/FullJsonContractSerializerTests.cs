using Pipaslot.Mediator.Http;

namespace Pipaslot.Mediator.Tests.Http
{
    public class FullJsonContractSerializerTests : ContractSerializerTestBase
    {
        protected override IContractSerializer CreateSerializer()
        {
            return new FullJsonContractSerializer();
        }
    }
}
