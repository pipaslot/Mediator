using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System.Text.Json;

namespace Pipaslot.Mediator.Http.Tests.Serialization.V3;

public class JsonContractSerializer_MalformedResponseTests : ContractSerializerBaseTest
{
    protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
    {
        var optionsMock = new Mock<IMediatorOptions>();
        return new JsonContractSerializer(provider, optionsMock.Object);
    }

    [Theory]
    [InlineData(@"{""Success"":true,""Results"":[""not-an-object""]}")]
    [InlineData(@"{""Success"":true,""Results"":[{}]}")]
    [InlineData(@"{""Success"":true,""Results"":[{""Name"":""x""}]}")]
    [InlineData(@"{""Success"":true,""Results"":[{""$type"":123}]}")]
    public void Response_ResultMalformed_ThrowsMediatorHttpException(string body)
    {
        var sut = CreateSerializer();

        Assert.Throws<MediatorHttpException>(() => sut.DeserializeResponse<Result>(body));
    }

    [Fact]
    public void Response_PrimitiveResultMissingValueProperty_ThrowsMediatorHttpException()
    {
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(int)));
        var body = $$"""{"Success":true,"Results":[{"$type":{{typeJson}}}]}""";
        var sut = CreateSerializer();

        Assert.Throws<MediatorHttpException>(() => sut.DeserializeResponse<int>(body));
    }

    [Fact]
    public void Response_PrimitiveResultWrongValuePropertyName_ThrowsMediatorHttpException()
    {
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(int)));
        var body = $$"""{"Success":true,"Results":[{"$type":{{typeJson}},"Foo":5}]}""";
        var sut = CreateSerializer();

        Assert.Throws<MediatorHttpException>(() => sut.DeserializeResponse<int>(body));
    }

    [Fact]
    public void Response_ArrayResultMissingItemsProperty_ThrowsMediatorHttpException()
    {
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(Result[])));
        var body = $$"""{"Success":true,"Results":[{"$type":{{typeJson}}}]}""";
        var sut = CreateSerializer();

        Assert.Throws<MediatorHttpException>(() => sut.DeserializeResponse<Result[]>(body));
    }

    [Fact]
    public void Response_ArrayResultWrongItemsPropertyName_ThrowsMediatorHttpException()
    {
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(Result[])));
        var body = $$"""{"Success":true,"Results":[{"$type":{{typeJson}},"Foo":[]}]}""";
        var sut = CreateSerializer();

        Assert.Throws<MediatorHttpException>(() => sut.DeserializeResponse<Result[]>(body));
    }

    [Fact]
    public void Response_NullablePrimitiveResult_Deserializes()
    {
        // result.GetType() on a boxed non-null Nullable<T> unwraps to T, so a Nullable<> $type only ever
        // arises from a hand-crafted payload like this one - exercises the Nullable-unwrapping branch of AsPrimitive.
        var typeJson = JsonSerializer.Serialize(ContractSerializerTypeHelper.GetIdentifier(typeof(int?)));
        var body = $$"""{"Success":true,"Results":[{"$type":{{typeJson}},"Value":5}]}""";
        var sut = CreateSerializer();

        var deserialized = sut.DeserializeResponse<int?>(body);

        Assert.Equal(5, deserialized.Result);
    }

    public class Result
    {
        public int Index { get; init; }
    }
}
