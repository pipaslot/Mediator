using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V2;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V2;

public class FullJsonContractSerializer_CredibilityTests : ContractSerializer_CredibilityTestBase
{
    protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
    {
        return new FullJsonContractSerializer(provider);
    }
}