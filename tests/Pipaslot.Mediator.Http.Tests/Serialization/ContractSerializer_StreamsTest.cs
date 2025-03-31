using Moq;
using Pipaslot.Mediator.Http.Configuration;
using Pipaslot.Mediator.Http.Serialization;
using Pipaslot.Mediator.Http.Serialization.V3;
using System.IO;
using System.Linq;
using System.Text;
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
            new(ToStream(streamContent1), fileName1), 
            new(ToStream(streamContent2), fileName2)
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
        Assert.Equal(streamContent1, FromStream(file1.Content));
        var file2 = deserialized.Files.Skip(1).First();
        Assert.Equal(fileName2, file2.Name);
        Assert.Equal(streamContent2, FromStream(file2.Content));
    }
    
    protected override IContractSerializer CreateSerializer(ICredibleProvider provider)
    {
        var optionsMock = new Mock<IMediatorOptions>();
        return new JsonContractSerializer(provider, optionsMock.Object);
    }

    private Stream ToStream(string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    private static string FromStream(Stream stream)
    {
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    private record FileUploadAction(FileDto[] Files) : IMessage;

    private record FileDto(Stream Content, string Name) : IMessage;
}