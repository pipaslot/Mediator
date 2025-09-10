namespace Pipaslot.Mediator.Http.Tests.Serialization;
public abstract class ContractSerializer_PrimitiveTypesTestBase : Pipaslot.Mediator.Http.Tests.Serialization.ContractSerializerBaseTest
{
    [Test]
    [Arguments(true)]
    [Arguments(false)]
    public void Boolean(bool result)
    {
        Test(result);
    }

    [Test]
    [Arguments(-100)]
    [Arguments(0)]
    [Arguments(100)]
    public void Integer(int result)
    {
        Test(result);
    }

    [Test]
    [Arguments(-100f)]
    [Arguments(0f)]
    [Arguments(100f)]
    public void Float(float result)
    {
        Test(result);
    }

    [Test]
    [Arguments("")]
    [Arguments("HAHA")]
    public void String(string result)
    {
        Test(result);
    }

    [Test]
    public void NullActionResult()
    {
        Test(new NullActionResult());
    }

    protected void Test<T>(T result)
        where T : notnull
    {
        var sut = CreateSerializer();
        var responseString = sut.SerializeResponse(new MediatorResponse(true, [result]));
        var deserialized = sut.DeserializeResponse<T>(responseString);
        Assert.Equal(result, deserialized.Result);
    }
}