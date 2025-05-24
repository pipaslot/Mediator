using System.IO;
using System.Text;

namespace Pipaslot.Mediator.Http.Serialization;

internal static class StreamExtensions
{
    internal static Stream ConvertToStream(this string s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

    internal static string ConvertToString(this Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}