using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using Xunit;
using System;

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
        public void Null_NotSupported()
        {
            // Null value and nullable types are not supported as we are not able to detect object type during processing the dynamic result collection
            Assert.Throws<NullReferenceException>(() =>
            {
                DateTime? value = null;
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
                Test(value);
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
            });
        }
        
        [Fact]
        public void DateOnly_NotSupported()
        {
            // DateOnly and TimeOnly is not part of .NET Standard 
            Assert.Throws<NotSupportedException>(() =>
            {
                DateOnly value = System.DateOnly.Parse("2023-10-10");
                Test(value);
            });
        }
        
        [Fact]
        public void TimeOnly_NotSupported()
        {
            // DateOnly and TimeOnly is not part of .NET Standard 
            Assert.Throws<NotSupportedException>(() =>
            {
                TimeOnly value = System.TimeOnly.Parse("10:30:15");
                Test(value);
            });
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