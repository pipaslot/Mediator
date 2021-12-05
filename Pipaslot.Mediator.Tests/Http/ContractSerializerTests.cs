using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http;

namespace Pipaslot.Mediator.Tests.Http
{
    public class ContractSerializerTests : ContractSerializerTestBase
    {
        protected override IContractSerializer CreateSerializer(PipelineConfigurator configurator)
        {
            return new ContractSerializer(configurator);
        }
    }
}
