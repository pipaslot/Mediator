using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;

namespace Pipaslot.Mediator.Http.Tests.Serialization
{
    public abstract class ContractSerializerBaseTest
    {
        protected Mock<ICredibleProvider> CredibleProviderMock = new();
        protected abstract IContractSerializer CreateSerializer(ICredibleProvider provider);
        protected IContractSerializer CreateSerializer()
        {
            return CreateSerializer(CredibleProviderMock.Object);
        }
    }
}
