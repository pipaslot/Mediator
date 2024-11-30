using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization;

public abstract class ContractSerializer_ArrayTestBase : ContractSerializerBaseTest
{
    [Fact]
    public void Response_ResultTypeIsArray_Deserialize()
    {
        var collection = new Result[] { new() { Index = 1 }, new() { Index = 2 } };
        DeserializeCollection(collection);
    }

    [Fact]
    public void Response_ResultTypeIsList_Deserialize()
    {
        var collection = new List<Result> { new() { Index = 1 }, new() { Index = 2 } };
        DeserializeCollection(collection);
    }

    private void DeserializeCollection<T>(T result) where T : ICollection<Result>
    {
        var sut = CreateSerializer();
        var responseString = sut.SerializeResponse(new MediatorResponse(true, new object[] { result }));
        var deserialized = sut.DeserializeResponse<T>(responseString);
        Assert.Equal(result.GetType(), deserialized.Result.GetType());
        Assert.Equal(result.First().Index, deserialized.Result.First().Index);
        Assert.Equal(result.Last().Index, deserialized.Result.Last().Index);
    }

    public class Result
    {
        public int Index { get; set; }
    }
}