using Pipaslot.Mediator.Configuration;
using Pipaslot.Mediator.Http;
using Pipaslot.Mediator.Http.Serialization;

namespace Pipaslot.Mediator.Tests.Http.Serialization
{
    public class FullJsonContractSerializerTests : ContractSerializerTestBase
    {
        protected override IContractSerializer CreateSerializer(PipelineConfigurator configurator)
        {
            return new FullJsonContractSerializer(configurator);
        }
    }
}
