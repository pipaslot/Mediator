using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System.IO;
using System.Linq;
using Xunit;

namespace Pipaslot.Mediator.Http.Tests.Serialization;

public class ContractSerializer_StreamTest : ContractSerializerBaseTest
{
    [Fact]
    public void StreamsAreExtractedDuringSerializationAndPlacedBackWhenDeserializing()
    {
        var streamContent1 = "Stream number one";
        var fileName1 = "Stream 1";

        var streamContent2 = "Stream number two";
        var fileName2 = "Stream 2";
        var files = new FileDto[]
        {
            new(streamContent1.ConvertToStream(), fileName1), 
            new(streamContent2.ConvertToStream(), fileName2)
        };
        var action = new FileUploadAction(files);
        var sut = CreateSerializer();
        var serialized = sut.SerializeRequest(action);
        Assert.Equal(2, serialized.Streams.Count);// Two stream are expected because two files were passed
        
        var deserialized = sut.DeserializeRequest(serialized.Json, serialized.Streams) as FileUploadAction;
        Assert.NotNull(deserialized);
        Assert.Equal(2, deserialized.Files.Length);

        var file1 = deserialized.Files.First();
        Assert.Equal(fileName1, file1.Name);
        Assert.Equal(streamContent1, file1.Content.ConvertToString());
        var file2 = deserialized.Files.Skip(1).First();
        Assert.Equal(fileName2, file2.Name);
        Assert.Equal(streamContent2, file2.Content.ConvertToString());
    }
    
    protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
    {
        var optionsMock = new Mock<IMediatorOptions>();
        return new JsonContractSerializer(provider, optionsMock.Object);
    }

    private record FileUploadAction(FileDto[] Files) : IMessage;

    private record FileDto(Stream Content, string Name) : IMessage;
}