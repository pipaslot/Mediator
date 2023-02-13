using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3
{
    public class JsonContractSerializer_PrimitiveTypesTests : ContractSerializer_PrimitiveTypesTestBase
    {
        protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
        {
            return new JsonContractSerializer(provider);
        }
    }
    }
