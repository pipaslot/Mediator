using NSubstitute;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;

namespace Pipaslot.Mediator.Http.Tests.Serialization;

public abstract class ContractSerializerBaseTest
{
    protected ICredibleProvider CredibleProvider = Substitute.For<ICredibleProvider>();
    protected abstract IContractSerializer CreateSerializer(ICredibleProvider provider);

    protected IContractSerializer CreateSerializer()
    {
        return CreateSerializer(CredibleProvider);
    }
}
