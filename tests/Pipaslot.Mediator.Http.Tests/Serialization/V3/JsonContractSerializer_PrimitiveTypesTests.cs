using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3;
public class JsonContractSerializer_PrimitiveTypesTests : Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializer_PrimitiveTypesTestBase
{
    protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
    {
        var optionsMock = new Mock<IMediatorOptions>();
        return new JsonContractSerializer(provider, optionsMock.Object);
    }

    [Test]
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

    [Test]
    public void DateOnly_Pass()
    {
        var value = DateOnly.Parse("2023-10-10");
        Test(value);
    }

    [Test]
    public void TimeOnly_Pass()
    {
        var value = TimeOnly.Parse("10:30:15");
        Test(value);
    }

    [Test]
    public void DateTime()
    {
        var value = System.DateTime.Now;
        Test(value);
    }

    [Test]
    public void DateTimeOffset()
    {
        var value = System.DateTimeOffset.Now;
        Test(value);
    }

    [Test]
    public void ByteArray()
    {
        var value = new byte[3]
        {
            1,
            2,
            3
        };
        Test(value);
    }

    [Test]
    public void Decimal()
    {
        var value = 5m;
        Test(value);
    }
}