using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization;

public abstract class ContractSerializer_PrimitiveTypesTestBase : ContractSerializerBaseTest
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Boolean(bool result)
    {
        Test(result);
    }

    [Theory]
    [InlineData(-100)]
    [InlineData(0)]
    [InlineData(100)]
    public void Integer(int result)
    {
        Test(result);
    }

    [Theory]
    [InlineData(-100f)]
    [InlineData(0f)]
    [InlineData(100f)]
    public void Float(float result)
    {
        Test(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("HAHA")]
    public void String(string result)
    {
        Test(result);
    }

    [Fact]
    public void NullActionResult()
    {
        Test(new NullActionResult());
    }

    protected void Test<T>(T result) where T : notnull
    {
        var sut = CreateSerializer();
        var responseString = sut.SerializeResponse(new MediatorResponse(true, [result]));
        var deserialized = sut.DeserializeResponse<T>(responseString);
        Assert.Equal(result, deserialized.Result);
    }
}