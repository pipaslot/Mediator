using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3
{
    public class JsonContractSerializer_PrimitiveTypesTests : ContractSerializer_PrimitiveTypesTestBase
    {
        protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
        {
            var optionsMock = new Mock<IMediatorOptions>();
            return new JsonContractSerializer(provider, optionsMock.Object);
        }
        
        
        [Fact]
        public void DateTime()
        {
            var value = System.DateTime.Now;
            Test(value);
        }
        
        [Fact]
        public void DateTimeOffset()
        {
            var value = System.DateTimeOffset.Now;
            Test(value);
        }
        
        [Fact]
        public void ByteArray()
        {
            var value = new byte[3]{ 1,2,3};
            Test(value);
        }
        
        [Fact]
        public void Decimal()
        {
            decimal value = 5m;
            Test(value);
        }

    }
}