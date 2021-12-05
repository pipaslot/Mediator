using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http;

namespace Pipaslot.Mediator.Tests.Http
{
    public class FullJsonContractSerializerTests : ContractSerializerTestBase
    {
        protected override IContractSerializer CreateSerializer(PipelineConfigurator configurator)
        {
            return new FullJsonContractSerializer(configurator);
        }
    }
}
