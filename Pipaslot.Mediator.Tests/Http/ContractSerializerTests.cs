using Pipaslot.Mediator.Http;

namespace Pipaslot.Mediator.Tests.Http
{
    public class ContractSerializerTests : ContractSerializerTestBase
    {
        protected override IContractSerializer CreateSerializer()
        {
            return new ContractSerializer();
        }
    }
}
