using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pipaslot.Mediator.Http.Serialization.V3.Converters;

/// <summary>
/// WARNING: Transient usage only
/// </summary>
internal class StreamExtractingConverter : JsonConverter<Stream>
{
    public StreamExtractingConverter(ICollection<StreamContract>? streams = null)
    {
        if (streams is not null)
        {
            foreach (var contract in streams)
            {
                _streamStorage[contract.Id] = contract.Stream;
            }
        }
    }
    private readonly Dictionary<string, Stream> _streamStorage = new();

    public override Stream? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
            throw new JsonException("Expected a string identifier for Stream.");

        var path = reader.GetString()!;

        // Retrieve the stored stream by its identifier
        return _streamStorage.TryGetValue(path, out var stream) ? stream : Stream.Null;
    }

    public override void Write(Utf8JsonWriter writer, Stream value, JsonSerializerOptions options)
    {
        var path = "stream:"+Guid.NewGuid(); // Generate a unique identifier

        // Store the stream in memory for later retrieval
        _streamStorage[path] = value;

        // Serialize only the identifier
        writer.WriteStringValue(path);
    }

    public ICollection<StreamContract> GetStreams() => _streamStorage
        .Select(p => new StreamContract(p.Key, p.Value))
        .ToList();
}